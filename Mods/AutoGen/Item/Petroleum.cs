namespace Eco.Mods.TechTree
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Eco.Gameplay.Blocks;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.World;
    using Eco.World.Blocks;
    using Eco.Gameplay.Pipes;
    using Eco.Gameplay.Pipes.LiquidComponents; 

    [RequiresSkill(typeof(PetrolRefiningSkill), 0)]   
    public partial class PetroleumRecipe : Recipe
    {
        public PetroleumRecipe()
        {
            this.Products = new CraftingElement[]
            {
                new CraftingElement<PetroleumItem>(),          
            };
            this.Ingredients = new CraftingElement[]
            {
                new CraftingElement<BarrelItem>(1), 
            };
            this.CraftMinutes = new MultiDynamicValue(MultiDynamicOps.Multiply
            , CreateCraftTimeValue(typeof(PetroleumRecipe), Item.Get<PetroleumItem>().UILink(), 30, typeof(PetrolRefiningSpeedSkill))   
            , new LayerModifiedValue(Eco.Simulation.WorldLayers.LayerNames.Oil,3)    
            );
            this.Initialize("Petroleum", typeof(PetroleumRecipe));

            CraftingComponent.AddRecipe(typeof(PumpJackObject), this);
        }
    }

    [Serialized]
    [Solid]
    public partial class PetroleumBlock :
        PickupableBlock     
    { }

    [Serialized]
    [MaxStackSize(10)]                                       
    [Weight(30000)]      
    [Fuel(40000)]          
    [Currency]              
    public partial class PetroleumItem :
    BlockItem<PetroleumBlock>
    {
        public override string FriendlyName { get { return "Petroleum"; } }
        public override string FriendlyNamePlural { get { return "Petroleum"; } } 
        public override string Description { get { return "A fossil fuel essential for manufacturing gasoline, plastics, and asphalt. It's extraction, transport, and burning all have environmental impacts."; } }

    }

}