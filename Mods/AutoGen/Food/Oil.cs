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
    [Weight(100)]                                          
    [Fuel(4000)]                                              
    public partial class OilItem :
        FoodItem            
    {
        public override string FriendlyName                     { get { return "Oil"; } }
        public override string FriendlyNamePlural               { get { return "Oil"; } } 
        public override string Description                      { get { return "A plant fat extracted for use in cooking."; } }

        private static Nutrients nutrition = new Nutrients()    { Carbs = 0, Fat = 15, Protein = 0, Vitamins = 0};
        public override float Calories                          { get { return 120; } }
        public override Nutrients Nutrition                     { get { return nutrition; } }
    }

    [RequiresSkill(typeof(MillProcessingSkill), 2)]    
    public partial class OilRecipe : Recipe
    {
        public OilRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<OilItem>(),
               
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<CerealGermItem>(typeof(MillProcessingEfficiencySkill), 30, MillProcessingEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = CreateCraftTimeValue(typeof(OilRecipe), Item.Get<OilItem>().UILink(), 5, typeof(MillProcessingSpeedSkill)); 
            this.Initialize("Oil", typeof(OilRecipe));
            CraftingComponent.AddRecipe(typeof(MillObject), this);
        }
    }
}