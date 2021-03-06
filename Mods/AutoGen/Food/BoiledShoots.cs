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
    public partial class BoiledShootsItem :
        FoodItem            
    {
        public override string FriendlyName                     { get { return "Boiled Shoots"; } }
        public override string FriendlyNamePlural               { get { return "Boiled Shoots"; } } 
        public override string Description                      { get { return "Boiled in water to remove the inherent bitterness, this shoot is much tastier."; } }

        private static Nutrients nutrition = new Nutrients()    { Carbs = 6, Fat = 3, Protein = 0, Vitamins = 11};
        public override float Calories                          { get { return 510; } }
        public override Nutrients Nutrition                     { get { return nutrition; } }
    }

    [RequiresSkill(typeof(CampfireCookingSkill), 3)]    
    public partial class BoiledShootsRecipe : Recipe
    {
        public BoiledShootsRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<BoiledShootsItem>(),
               
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<FireweedShootsItem>(typeof(CampfireCookingEfficiencySkill), 5, CampfireCookingEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = CreateCraftTimeValue(typeof(BoiledShootsRecipe), Item.Get<BoiledShootsItem>().UILink(), 2, typeof(CampfireCookingSpeedSkill)); 
            this.Initialize("Boiled Shoots", typeof(BoiledShootsRecipe));
            CraftingComponent.AddRecipe(typeof(CampfireObject), this);
        }
    }
}