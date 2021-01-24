using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LM.Models;
using System.Threading;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Components.Authorization;

namespace LM.Web.Pages
{
    public class LessonBase : ComponentBase
    {
        private SqlConnection Connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LM.Web;Trusted_Connection=True;MultipleActiveResultSets=true");

        [Parameter]
        public int LessonId { get; set; }

        public string LessonName { get; set; }
        public bool CustomLesson { get; set; }
        public Deck LessonDeck { get; set; }
        public Question CurrentQuestion { get; set; }
        public int QuestionIndex { get; set; }

        private string UserName { get; set; }

        [Inject]
        private NavigationManager NM { get; set; }
        [Inject]
        private IJSRuntime JSR { get; set; }

        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }

        private static Action NextQuestionAction { get; set; }
        private static Action UpdateStateAction { get; set; }

        [JSInvokable]
        public static void NextQuestionCaller()
        {
            NextQuestionAction.Invoke();
        }

        [JSInvokable]
        public static void UpdateStateCaller()
        {
            UpdateStateAction.Invoke();
        }

        protected override void OnInitialized()
        {
            var uri = NM.ToAbsoluteUri(NM.Uri);

            var queryStrings = QueryHelpers.ParseQuery(uri.Query);
            if (queryStrings.TryGetValue("custom", out var isCustom))
            {
                if (isCustom.Equals("true")) CustomLesson = true;
                else CustomLesson = false;
            }

            GetUserInfo();

            if (CustomLesson)
            {
                LoadCustomLessonDeck();
            }
            else
            {
                LoadStandardLessonDeck();
            }
            NextQuestionAction = NextQuestion;
            UpdateStateAction = UpdateState;

            base.OnInitialized();
        }

        private async void GetUserInfo()
        {
            var user = (await AuthState).User;
            if (user.Identity.IsAuthenticated)
            {
                UserName = user.Identity.Name;
            }
            else
            {
                UserName = await JSR.InvokeAsync<string>("GetGuestCookieValue");
            }
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                NextQuestion();
            }
            return base.OnAfterRenderAsync(firstRender);
        }

        private void LoadStandardLessonDeck()
        {
            LessonDeck = new Deck(0, "Unit " + LessonId, CardDB.CardsFromSet(LessonId));
        }

        private void LoadCustomLessonDeck()
        {
            Connection.Open();

            SqlCommand sqlcmd = new SqlCommand(@"select DeckList from dbo.AspNetUsers where UserName = @UserName", Connection);
            sqlcmd.Parameters.AddWithValue("@UserName", UserName);
            SqlDataReader rdr = sqlcmd.ExecuteReader();

            List<Deck> UserDecks = null;
            if (rdr.Read())
            {
                XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
                TextReader xmlrdr = new StringReader(rdr.GetString(0));
                UserDecks = (List<Deck>)xsr.Deserialize(xmlrdr);
            }

            foreach (Deck deck in UserDecks)
            {
                if (deck.Id == LessonId)
                {
                    LessonDeck = deck;
                    break;
                }
            }

            rdr.Close();
            sqlcmd.Dispose();
            Connection.Close();
        }

        private void SaveCustomLessonDeck()
        {
            XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
            StringWriter stwrtr = new StringWriter();
            XmlWriter xmlwrtr = XmlWriter.Create(stwrtr);
            xsr.Serialize(xmlwrtr, LessonDeck);

            string UserDeckXML = stwrtr.ToString();

            Connection.Open();

            SqlCommand sqlcmd = new SqlCommand("update dbo.AspNetUsers set DeckList = @DeckList where UserName = @UserName", Connection);
            sqlcmd.Parameters.AddWithValue("@DeckList", UserDeckXML);
            sqlcmd.Parameters.AddWithValue("@UserName", UserName);

            sqlcmd.ExecuteNonQuery();
            sqlcmd.Dispose();

            Connection.Close();
        }

        public void UpdateState()
        {
            StateHasChanged();
        }

        public async void NextQuestion()
        {
            QuestionIndex++;

            if (QuestionIndex < LessonDeck.Questions.Count)
            {
                CurrentQuestion = LessonDeck.Questions[QuestionIndex - 1];

                switch (CurrentQuestion.Type)
                {
                    case QType.CharMC:
                    case QType.MorseMC:
                        await JSR.InvokeVoidAsync("SetupMCQuestion", CurrentQuestion.GetFaceImage(), CurrentQuestion.GetBackImage(), CurrentQuestion.CorrectAnswerIndex);
                        break;
                    case QType.MorseInput:
                        await JSR.InvokeVoidAsync("SetupInputQuestion", CurrentQuestion.Card.Morse, CurrentQuestion.GetFaceImage(), CurrentQuestion.GetBackImage());
                        break;
                    case QType.Intro:
                        await JSR.InvokeVoidAsync("SetupIntroQuestion", CurrentQuestion.GetFaceImage(), CurrentQuestion.GetBackImage());
                        break;
                }

                SaveCustomLessonDeck();
            }
            else
            {
                // do something...
            }
        }
    }
}
