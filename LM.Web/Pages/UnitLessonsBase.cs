using LM.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LM.Web.Pages
{
    public class UnitLessonsBase : ComponentBase
    {
        public IEnumerable<UnitCard> Units { get; set; }
        public string UserName { get; set; }

        protected override void OnInitialized()
        {
            LoadUnits();
            base.OnInitialized();
        }

        public void LoadUnits()
        {
            Units = new List<UnitCard>
            {
                new UnitCard("A-E", "lessons/1?custom=false"),
                new UnitCard("F-J", "lessons/2?custom=false"),
                new UnitCard("K-O", "lessons/3?custom=false"),
                new UnitCard("P-T", "lessons/4?custom=false"),
                new UnitCard("U-Z", "lessons/5?custom=false"),
                new UnitCard("0-4", "lessons/6?custom=false"),
                new UnitCard("5-9", "lessons/7?custom=false")
            };
        }
    }

    public class UnitCard
    {
        public string Name { get; set; }
        public string Route { get; set; }

        public UnitCard(string Name, string Route)
        {
            this.Name = Name;
            this.Route = Route;
        }
    }
}
