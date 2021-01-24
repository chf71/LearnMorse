using LM.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LM.Web.Pages
{

    public class CustomLessonsBase : ComponentBase
    {
        private SqlConnection connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LM.Web;Trusted_Connection=True;MultipleActiveResultSets=true");
        private string UserName { get; set; }
        public List<Deck> UserDecks { get; set; } = new List<Deck>();
        public List<string> CustomDeck { get; set; } = new List<string>();
        public List<string> MasterCharDeck { get; set; } = new List<string>(CardDB.AllCharNames);
        public List<string> MasterNumDeck { get; set; } = new List<string>(CardDB.AllNumNames);
        public string DeckName { get; set; } = string.Empty;

        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }
        [Inject]
        private IJSRuntime JSR { get; set; }

        protected async override Task OnInitializedAsync()
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

            LoadUserDecks();
        }

        public void LoadUserDecks()
        {
            connection.Open();

            SqlCommand sqlcmd = new SqlCommand(@"select CustomLessons from dbo.AspNetUsers where UserName = @UserName", connection);
            sqlcmd.Parameters.AddWithValue("@UserName", UserName);
            SqlDataReader rdr = sqlcmd.ExecuteReader();

            if (rdr.Read())
            {
                XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
                TextReader xmlrdr = new StringReader(rdr.GetString(0));
                UserDecks = (List<Deck>)xsr.Deserialize(xmlrdr);
            }

            rdr.Close();
            sqlcmd.Dispose();
            connection.Close();
        }

        private void UpdateDeck()
        {
            XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
            StringWriter stwrtr = new StringWriter();
            XmlWriter xmlwrtr = XmlWriter.Create(stwrtr);
            xsr.Serialize(xmlwrtr, UserDecks);

            string UserDeckXML = stwrtr.ToString();

            connection.Open();

            SqlCommand sqlcmd = new SqlCommand("update dbo.AspNetUsers set CustomLessons = @DeckList where UserName = @UserName", connection);
            sqlcmd.Parameters.AddWithValue("@DeckList", UserDeckXML);
            sqlcmd.Parameters.AddWithValue("@UserName", UserName);

            sqlcmd.ExecuteNonQuery();
            sqlcmd.Dispose();

            connection.Close();
        }

        public void DeleteDeck(int index)
        {
            UserDecks.RemoveAt(index);

            UpdateDeck();
        }

        public void AddCharToDeck(string name)
        {
            if (CustomDeck.Count < 6)
            {
                CustomDeck.Add(name);
                MasterCharDeck.Remove(name);
            }
        }

        public void AddNumToDeck(string name)
        {
            if (CustomDeck.Count < 6)
            {
                CustomDeck.Add(name);
                MasterNumDeck.Remove(name);
            }
        }

        public void RemoveFromDeck(string name)
        {
            CustomDeck.Remove(name);
            if (int.TryParse(name, out _))
            {
                MasterNumDeck.Add(name);
                MasterNumDeck.Sort();
            }
            else
            {
                MasterCharDeck.Add(name);
                MasterCharDeck.Sort();
            }
        }

        public async void CreateDeck()
        {
            int newId = 1;
            if (UserDecks.Count > 0) newId = UserDecks[UserDecks.Count - 1].Id + 1;

            if (DeckName.Equals(string.Empty))
            {
                DeckName = "Custom Deck " + newId;
            }

            Deck deck = new Deck(newId, DeckName, CustomDeck);

            UserDecks.Add(deck);

            await JSR.InvokeVoidAsync("HideDeckBuilder");

            RemoveAllFromDeck();
            DeckName = string.Empty;

            UpdateDeck();
        }

        public void RemoveAllFromDeck()
        {
            MasterCharDeck = new List<string>(CardDB.AllCharNames);
            MasterNumDeck = new List<string>(CardDB.AllNumNames);
            CustomDeck = new List<string>();
        }
    }
}