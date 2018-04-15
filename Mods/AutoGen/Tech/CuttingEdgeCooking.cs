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
    [RequiresSkill(typeof(ChefSkill), 0)]    
    public partial class CuttingEdgeCookingSkill : Skill
    {
        public override string FriendlyName { get { return "Cutting Edge Cooking"; } }
        public override string Description { get { return Localizer.Do(""); } }

        public static int[] SkillPointCost = { 1, 1, 1, 1, 1 };
        public override int RequiredPoint { get { return this.Level < this.MaxLevel ? SkillPointCost[this.Level] : 0; } }
        public override int PrevRequiredPoint { get { return this.Level - 1 >= 0 && this.Level - 1 < this.MaxLevel ? SkillPointCost[this.Level - 1] : 0; } }
        public override int MaxLevel { get { return 1; } }
    }

    [Serialized]
    public partial class CuttingEdgeCookingSkillBook : SkillBook<CuttingEdgeCookingSkill, CuttingEdgeCookingSkillScroll>
    {
        public override string FriendlyName { get { return "Cutting Edge Cooking Skill Book"; } }
    }

    [Serialized]
    public partial class CuttingEdgeCookingSkillScroll : SkillScroll<CuttingEdgeCookingSkill, CuttingEdgeCookingSkillBook>
    {
        public override string FriendlyName { get { return "Cutting Edge Cooking Skill Scroll"; } }
    }

    [RequiresSkill(typeof(AdvancedCookingSkill), 0)] 
    public partial class CuttingEdgeCookingSkillBookRecipe : Recipe
    {
        public CuttingEdgeCookingSkillBookRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<CuttingEdgeCookingSkillBook>(),
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<SteelItem>(typeof(ResearchEfficiencySkill), 50, ResearchEfficiencySkill.MultiplicativeStrategy),
                new CraftingElement<YeastItem>(typeof(ResearchEfficiencySkill), 50, ResearchEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = new ConstantValue(60);

            this.Initialize("Cutting Edge Cooking Skill Book", typeof(CuttingEdgeCookingSkillBookRecipe));
            CraftingComponent.AddRecipe(typeof(ResearchTableObject), this);
        }
    }
}
