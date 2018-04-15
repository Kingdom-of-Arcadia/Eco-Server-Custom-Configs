namespace Eco.Mods.TechTree
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Mods.TechTree;
    using Eco.Shared.Items;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.Shared.View;

    [Serialized]
    [Weight(0)]
    [Category("Hidden")]
    public partial class BlinkyEyelidsItem :
        ClothingItem
    {
        public override string FriendlyName { get { return "Blinky Eyelids"; } }
        public override string Description  { get { return "Eyes"; } }
        public override string Slot         { get { return ClothingSlot.Eyelids; } } 
    }
}