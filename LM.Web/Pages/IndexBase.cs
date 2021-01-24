using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Cryptography;
using Microsoft.JSInterop;

namespace LM.Web.Pages
{
    public class IndexBase : ComponentBase
    {

        private string connectionString = "Server=(localdb)\\mssqllocaldb;Database=LM.Web;Trusted_Connection=True;MultipleActiveResultSets=true";

        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }
        [Inject]
        private IJSRuntime JSR { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            var authState = await AuthState;

            // check if user is either 1) logged in or 2) already has a guest cookie set
            if (!authState.User.Identity.IsAuthenticated && !(await JSR.InvokeAsync<bool>("CheckGuestCookie")) )
            {
                // if neither, then they're new, so give them a guest cookie
                Aes aes = Aes.Create();
                aes.GenerateKey();
                aes.GenerateIV();

                string keyValue = Convert.ToBase64String(aes.Key);

                // js set cookie
                await JSR.InvokeVoidAsync("SetGuestCookie", keyValue);
            }
        }

        public List<string> GetUsers()
        {
            using ( SqlConnection connection = new SqlConnection(connectionString) )
            {
                List<string> users = new List<string>();

                connection.Open();

                SqlCommand sqlcmd = new SqlCommand(@"select * from dbo.AspNetUsers", connection);

                SqlDataReader rdr = sqlcmd.ExecuteReader();

                if (rdr.Read())
                {
                    users.Add(rdr.GetString(0) + " " + rdr.GetString(1));
                }

                return users;
            }
        }
    }

}
