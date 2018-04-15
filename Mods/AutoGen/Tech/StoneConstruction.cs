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
    [RequiresSkill(typeof(MasonSkill), 0)]    
    public partial class StoneConstructionSkill : Skill
    {
        public override string FriendlyName { get { return "Stone Construction"; } }
        public override string Description { get { return Localizer.Do(""); } }

        public static int[] SkillPointCost = { 1, 1, 1, 1, 1 };
        public override int RequiredPoint { get { return this.Level < this.MaxLevel ? SkillPointCost[this.Level] : 0; } }
        public override int PrevRequiredPoint { get { return this.Level - 1 >= 0 && this.Level - 1 < this.MaxLevel ? SkillPointCost[this.Level - 1] : 0; } }
        public override int MaxLevel { get { return 1; } }
    }

    [Serialized]
    public partial class StoneConstructionSkillBook : SkillBook<StoneConstructionSkill, StoneConstructionSkillScroll>
    {
        public override string FriendlyName { get { return "Stone Construction Skill Book"; } }
    }

    [Serialized]
    public partial class StoneConstructionSkillScroll : SkillScroll<StoneConstructionSkill, StoneConstructionSkillBook>
    {
        public override string FriendlyName { get { return "Stone Construction Skill Scroll"; } }
    }

    [RequiresSkill(typeof(MortaringSkill), 0)] 
    public partial class StoneConstructionSkillBookRecipe : Recipe
    {
        public StoneConstructionSkillBookRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<StoneConstructionSkillBook>(),
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<MortaredStoneItem>(typeof(ResearchEfficiencySkill), 10, ResearchEfficiencySkill.MultiplicativeStrategy), 
            };
            this.CraftMinutes = new ConstantValue(5);

            this.Initialize("Stone Construction Skill Book", typeof(StoneConstructionSkillBookRecipe));
            CraftingComponent.AddRecipe(typeof(ResearchTableObject), this);
        }
    }
}
