// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.ComponentModel;
using System.Linq;
using Eco.Core.Controller;
using Eco.Gameplay;
using Eco.Gameplay.Interactions;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems;
using Eco.Gameplay.Systems.Chat;
using Eco.Shared.Items;
using Eco.Shared.Math;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Shared.Serialization;
using Eco.Shared.Services;
using Eco.Shared.Utils;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.World;
using Eco.World.Blocks;
using Eco.Gameplay.DynamicValues;

[Serialized]
public partial class ArcadiaManualItem : Item {

	public static string save = System.IO.Path.Combine(
		System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ArchMod"
	);
	public static string Manual = "";

	public override string FriendlyName  { get { return "Arcadia Player Manual"; } }

	static ArcadiaManualItem() {
		
	}

	public override string Description {
		get {
			Manual = read_txt_file("arcadia_player_manual.txt");
			return Manual;
		}
	}

	private static string read_txt_file (string filename) {
		if (!File.Exists( save + "/" + filename ))
		return string.Empty;

		var content = string.Empty;
		using (StreamReader file = new StreamReader( save + "/" + filename )) {
			content = file.ReadToEnd();
		}
		return content;
	}
}

