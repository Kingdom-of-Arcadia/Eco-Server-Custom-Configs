namespace Eco.Mods.TechTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Eco.Core.Utils;
    using Eco.Core.Utils.AtomicAction;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Property;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.Services;
    using Eco.Shared.Utils;
    using Gameplay.Systems.Tooltip;

    [Serialized]
    [RequiresSkill(typeof(FarmerSkill), 0)]    
    public partial class FertilizersSkill : Skill
    {
        public override string FriendlyName { get { return "Fertilizers"; } }
        public override string Description { get { return Localizer.Do(""); } }

        public static int[] SkillPointCost = { 1, 1, 1, 1, 1 };
        public override int RequiredPoint { get { return this.Level < this.MaxLevel ? SkillPointCost[this.Level] : 0; } }
        public override int PrevRequiredPoint { get { return this.Level - 1 >= 0 && this.Level - 1 < this.MaxLevel ? SkillPointCost[this.Level - 1] : 0; } }
        public override int MaxLevel { get { return 1; } }
    }

    [Serialized]
    public partial class FertilizersSkillBook : SkillBook<FertilizersSkill, FertilizersSkillScroll>
    {
        public override string FriendlyName { get { return "Fertilizers Skill Book"; } }
    }

    [Serialized]
    public partial class FertilizersSkillScroll : SkillScroll<FertilizersSkill, FertilizersSkillBook>
    {
        public override string FriendlyName { get { return "Fertilizers Skill Scroll"; } }
    }

    [RequiresSkill(typeof(FarmingSkill), 0)] 
    public partial class FertilizersSkillBookRecipe : Recipe
    {
        public FertilizersSkillBookRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<FertilizersSkillBook>(),
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<DirtItem>(typeof(ResearchEfficiencySkill), 10, ResearchEfficiencySkill.MultiplicativeStrategy),
                new CraftingElement<PlantFibersItem>(typeof(ResearchEfficiencySkill), 30, ResearchEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = new ConstantValue(5);

            this.Initialize("Fertilizers Skill Book", typeof(FertilizersSkillBookRecipe));
            CraftingComponent.AddRecipe(typeof(ResearchTableObject), this);
        }
    }
}
