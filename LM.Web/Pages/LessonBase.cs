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
        public Deck LessonDeck { get; set; }
        public Question CurrentQuestion { get; set; }
        public int QuestionIndex { get; set; }

        private List<Deck> UserDecks { get; set; }
        
        private enum LessonType { StandardLesson, CustomLesson, StandardPractice, CustomPractice };

        private LessonType Type { get; set; }

        private string UserName { get; set; }

        [Inject]
        private NavigationManager NM { get; set; }
        [Inject]
        private IJSRuntime JSR { get; set; }

        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }

        private static Action<bool> NextQuestionAction { get; set; }
        private static Action UpdateStateAction { get; set; }

        [JSInvokable]
        public static void NextQuestionCaller()
        {
            NextQuestionAction.Invoke(true);
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

            GetLessonType(queryStrings);
            GetUserInfo();
            LoadLessonDeck();

            queryStrings.TryGetValue("reset", out var reset);
            if (reset.Equals("true")) LessonDeck.Progress = 0;

            NextQuestionAction = NextQuestion;
            UpdateStateAction = UpdateState;

            base.OnInitialized();
        }

        private void GetLessonType(Dictionary<string, Microsoft.Extensions.Primitives.StringValues> queryStrings)
        {
            queryStrings.TryGetValue("type", out var lessonType);
            switch (lessonType)
            {
                case "standardLesson":
                    Type = LessonType.StandardLesson;
                    break;
                case "customLesson":
                    Type = LessonType.CustomLesson;
                    break;
                case "standardPractice":
                    Type = LessonType.StandardPractice;
                    break;
                case "customPractice":
                    Type = LessonType.CustomPractice;
                    break;
            }
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
                NextQuestion(false);
            }
            return base.OnAfterRenderAsync(firstRender);
        }

        private void LoadLessonDeck()
        {
            Connection.Open();

            SqlCommand sqlcmd = null;
            switch (Type)
            {
                case LessonType.StandardLesson:
                    sqlcmd = new SqlCommand(@"select UnitLessons from dbo.AspNetUsers where UserName = @UserName", Connection);
                    break;
                case LessonType.CustomLesson:
                    sqlcmd = new SqlCommand(@"select CustomLessons from dbo.AspNetUsers where UserName = @UserName", Connection);
                    break;
            }
            sqlcmd.Parameters.AddWithValue("@UserName", UserName);

            SqlDataReader rdr = sqlcmd.ExecuteReader();
            if (rdr.Read())
            {
                XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
                TextReader xmlrdr = new StringReader(rdr.GetString(0));
                UserDecks = (List<Deck>)xsr.Deserialize(xmlrdr);
            }

            switch (Type)
            {
                case LessonType.StandardLesson:
                    LessonDeck = UserDecks[LessonId - 1];
                    break;
                case LessonType.CustomLesson:
                    foreach (Deck deck in UserDecks)
                    {
                        if (deck.Id == LessonId)
                        {
                            LessonDeck = deck;
                            break;
                        }
                    }
                    break;
            }

            rdr.Close();
            sqlcmd.Dispose();
            Connection.Close();
        }

        private void SaveLessonDeck()
        {
            XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
            StringWriter stwrtr = new StringWriter();
            XmlWriter xmlwrtr = XmlWriter.Create(stwrtr);
            xsr.Serialize(xmlwrtr, UserDecks);

            string UserDecksXML = stwrtr.ToString();

            Connection.Open();

            SqlCommand sqlcmd = null;
            switch (Type)
            {
                case LessonType.StandardLesson:
                    sqlcmd = new SqlCommand("update dbo.AspNetUsers set UnitLessons = @DeckList where UserName = @UserName", Connection);
                    break;
                case LessonType.CustomLesson:
                    sqlcmd = new SqlCommand("update dbo.AspNetUsers set CustomLessons = @DeckList where UserName = @UserName", Connection);
                    break;
            }
            sqlcmd.Parameters.AddWithValue("@DeckList", UserDecksXML);
            sqlcmd.Parameters.AddWithValue("@UserName", UserName);

            sqlcmd.ExecuteNonQuery();
            sqlcmd.Dispose();

            Connection.Close();
        }

        public void UpdateState()
        {
            StateHasChanged();
        }

        public async void NextQuestion(bool calledNext)
        {
            if (calledNext)
            {
                if (Type == LessonType.StandardLesson || Type == LessonType.CustomLesson)
                {
                    LessonDeck.Progress++;
                    SaveLessonDeck();
                }
            }

            QuestionIndex = LessonDeck.Progress;

            if (QuestionIndex < LessonDeck.Questions.Count)
            {
                CurrentQuestion = LessonDeck.Questions[QuestionIndex];

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
            }
            else
            {
                // reroute to page we came from
            }
        }
    }
}
