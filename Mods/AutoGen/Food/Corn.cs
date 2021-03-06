namespace Eco.Mods.TechTree
{
    using System.Collections.Generic;
    using System.Linq;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Mods.TechTree;
    using Eco.Shared.Items;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.Shared.View;
    
    [Serialized]
    [Weight(10)]                                          
    [Yield(typeof(CornItem), typeof(GrasslandGathererSkill), new float[] {1f, 1.2f, 1.4f, 1.6f, 1.8f, 2f})]      
    public partial class CornItem :
        FoodItem            
    {
        public override string FriendlyName                     { get { return "Corn"; } }
        public override string FriendlyNamePlural               { get { return "Corn"; } } 
        public override string Description                      { get { return "A warmly colored kernel studded vegetable."; } }

        private static Nutrients nutrition = new Nutrients()    { Carbs = 5, Fat = 0, Protein = 2, Vitamins = 1};
        public override float Calories                          { get { return 230; } }
        public override Nutrients Nutrition                     { get { return nutrition; } }
    }

}