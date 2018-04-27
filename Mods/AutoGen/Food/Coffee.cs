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
    [Weight(300)]
    public partial class BlackCoffeeItem : FoodItem {
        public override string FriendlyName                     { get { return "Black Coffee"; } }
        public override string Description                      { get { return "A strongly brewed cup of coffee."; } }

        public override float Calories                          { get { return 20; } }

        private static Nutrients nutrition = new Nutrients()    { Carbs = 0, Fat = 0, Protein = 0, Vitamins = 2};
        public override Nutrients Nutrition                     { get { return nutrition; } }

        private static Dictionary<UserStatType, float> flatStats = new Dictionary<UserStatType, float>() {
			{ UserStatType.MovementSpeed, 0.5f },
		};
		public Dictionary<UserStatType, float> GetFlatStats() { return flatStats; }

		public override bool OnUsed (Player player) {

			static Dictionary<UserStatType, IDynamicValue> dynamicValuesDict = new Dictionary<UserStatType, IDynamicValue>() {
				{
					UserStatType.MovementSpeed, new MultiDynamicValue(MultiDynamicOps.Sum,
						CreateSmv(0.5f, CalorieEfficiencySkill.AdditiveStrategy, typeof(CalorieEfficiencySkill), Localizer.Do("Moving faster for a time")),
						new ConstantValue(0))
				},
			};

			private static SkillModifiedValue CreateSmv(float startValue, ModificationStrategy strategy, Type skillType, LocString benefitsDescription) {
				SkillModifiedValue smv = new SkillModifiedValue(startValue, strategy, skillType, benefitsDescription);
				SkillModifiedValueManager.AddSkillBenefit(Localizer.Do("You"), smv);
				return smv;
			}


			//base.OnUsed(player);
			return true;
		}
    }

    [Serialized]
    [Weight(300)]
    public partial class LatteCoffeeItem : FoodItem {
        public override string FriendlyName                     { get { return "Latte Coffee"; } }
        public override string Description                      { get { return "A strongly brewed cup of coffee with steamed rice-milk."; } }

        private static Nutrients nutrition = new Nutrients()    { Carbs = 4, Fat = 2, Protein = 0, Vitamins = 2};
        public override float Calories                          { get { return 40; } }
        public override Nutrients Nutrition                     { get { return nutrition; } }
    }

    [Serialized]
    [Weight(300)]
    public partial class ArcadianoCoffeeItem : FoodItem {
        public override string FriendlyName                     { get { return "Cafe Arcadiano Coffee"; } }
        public override string Description                      { get { return "A strongly brewed cup of coffee with steamed rice-milk and sugar: an Arcadian favorite."; } }

        private static Nutrients nutrition = new Nutrients()    { Carbs = 6, Fat = 3, Protein = 0, Vitamins = 2};
        public override float Calories                          { get { return 60; } }
        public override Nutrients Nutrition                     { get { return nutrition; } }
    }

    [RequiresSkill(typeof(CulinaryArtsSkill), 1)]
    public partial class BlackCoffeeRecipe : Recipe
    {
        public BlackCoffeeRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<BlackCoffeeItem>(),
               
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<FireweedShootsItem>(typeof(CulinaryArtsEfficiencySkill), 15, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
                new CraftingElement<BeansItem>(typeof(CulinaryArtsEfficiencySkill), 5, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = CreateCraftTimeValue(typeof(BlackCoffeeRecipe), Item.Get<BlackCoffeeItem>().UILink(), 5, typeof(CulinaryArtsSpeedSkill)); 
            this.Initialize("Black Coffee", typeof(BlackCoffeeRecipe));
            CraftingComponent.AddRecipe(typeof(KitchenObject), this);
        }
    }

    [RequiresSkill(typeof(CulinaryArtsSkill), 2)]
    public partial class LatteCoffeeRecipe : Recipe
    {
        public LatteCoffeeRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<LatteCoffeeItem>(),
               
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<RiceItem>(typeof(CulinaryArtsEfficiencySkill), 15, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
                new CraftingElement<FireweedShootsItem>(typeof(CulinaryArtsEfficiencySkill), 15, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
                new CraftingElement<BeansItem>(typeof(CulinaryArtsEfficiencySkill), 5, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = CreateCraftTimeValue(typeof(LatteCoffeeRecipe), Item.Get<LatteCoffeeItem>().UILink(), 5, typeof(CulinaryArtsSpeedSkill)); 
            this.Initialize("Latte Coffee", typeof(LatteCoffeeRecipe));
            CraftingComponent.AddRecipe(typeof(KitchenObject), this);
        }
    }

    [RequiresSkill(typeof(CulinaryArtsSkill), 3)]
    public partial class ArcadianoCoffeeRecipe : Recipe
    {
        public ArcadianoCoffeeRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<ArcadianoCoffeeItem>(),
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<RiceItem>(typeof(CulinaryArtsEfficiencySkill), 15, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
                new CraftingElement<FireweedShootsItem>(typeof(CulinaryArtsEfficiencySkill), 15, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
                new CraftingElement<BeansItem>(typeof(CulinaryArtsEfficiencySkill), 5, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
                new CraftingElement<SugarItem>(typeof(CulinaryArtsEfficiencySkill), 2, CulinaryArtsEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = CreateCraftTimeValue(typeof(ArcadianoCoffeeRecipe), Item.Get<ArcadianoCoffeeItem>().UILink(), 5, typeof(CulinaryArtsSpeedSkill)); 
            this.Initialize("Cafe Arcadiano Coffee", typeof(ArcadianoCoffeeRecipe));
            CraftingComponent.AddRecipe(typeof(KitchenObject), this);
        }
    }

}
