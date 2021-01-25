using LM.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LM.Web.Pages
{
    public class UnitLessonsBase : ComponentBase
    {
        private SqlConnection connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=LM.Web;Trusted_Connection=True;MultipleActiveResultSets=true");
        public List<Deck> UnitDecks { get; set; }
        public string UserName { get; set; }

        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await LoadUnits();
        }

        public async Task LoadUnits()
        {
            var user = (await AuthState).User;
            if (user.Identity.IsAuthenticated)
            {
                UserName = user.Identity.Name;
                connection.Open();

                SqlCommand sqlcmd = new SqlCommand(@"select UnitLessons from dbo.AspNetUsers where UserName = @UserName and UnitLessons is not null", connection);
                sqlcmd.Parameters.AddWithValue("@UserName", UserName);
                SqlDataReader rdr = sqlcmd.ExecuteReader();

                if (rdr.HasRows && rdr.Read())
                {
                    XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
                    TextReader xmlrdr = new StringReader(rdr.GetString(0));
                    UnitDecks = (List<Deck>)xsr.Deserialize(xmlrdr);
                }
                else
                {
                    // create them
                    UnitDecks = new List<Deck>();
                    for (int i = 1; i < 8; i++)
                    {
                        UnitDecks.Add(Deck.DeckFromSet(i));
                    }

                    XmlSerializer xsr = new XmlSerializer(typeof(List<Deck>));
                    StringWriter stwrtr = new StringWriter();
                    XmlWriter xmlwrtr = XmlWriter.Create(stwrtr);
                    xsr.Serialize(xmlwrtr, UnitDecks);

                    string UnitDecksXML = stwrtr.ToString();

                    sqlcmd = new SqlCommand("update dbo.AspNetUsers set UnitLessons = @DeckList where UserName = @UserName", connection);
                    sqlcmd.Parameters.AddWithValue("@DeckList", UnitDecksXML);
                    sqlcmd.Parameters.AddWithValue("@UserName", UserName);
                    sqlcmd.ExecuteNonQuery();
                }

                rdr.Close();
                sqlcmd.Dispose();
                connection.Close();
            }
        }
    }
}
