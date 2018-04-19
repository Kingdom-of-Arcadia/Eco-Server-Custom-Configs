/*
    Title: ArchMod
    Desc: A Governance/Utility Eco Mod
    Author: Archpoet <@Archpoet#0047>
    Date: 2018.04.01
*/

namespace Eco.Mods {

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Text.RegularExpressions;

	using Eco.Core.Agents;
	using Eco.Core.Plugins.Interfaces;
	using Eco.Core.Serialization;
	using Eco.Core.Utils;
	using Eco.Core.Utils.AtomicAction;
	using Eco.Gameplay;
	using Eco.Gameplay.Components;
	using Eco.Gameplay.DynamicValues;
	using Eco.Gameplay.Economy;
	using Eco.Gameplay.Items;
	using Eco.Gameplay.Players;
	using Eco.Gameplay.Property;
	using Eco.Gameplay.Skills;
	using Eco.Gameplay.Systems;
	using Eco.Gameplay.Systems.Chat;
	using Eco.Gameplay.Systems.Tooltip;
	using Eco.Gameplay.Systems.TextLinks;
	using Eco.Mods.TechTree;
	using Eco.Shared.Localization;
	using Eco.Shared.Math;
	using Eco.Shared.Networking;
	using Eco.Shared.Serialization;
	using Eco.Shared.Services;
	using Eco.Shared.Utils;
	using Eco.Shared.View;
	using Eco.World;

	public class ArchModCommands : IChatCommandHandler /*, IInitializablePlugin */ {
		// Sync
		public static int groups_sync_interval = (150) * 1000; // 2.5 mins
		public static Timer groups_sync_timer;
		public static int notify_sync_interval = (10) * 1000; // 10 secs
		public static Timer notify_sync_timer;

		// Motd
		public static int motd_interval = (2700) * 1000; // 45 mins
		public static Timer motd_timer;
		public static List <string> messages = new List<string>();

		// Mint
		public static float mint_pool = 50000f;
		public static int mint_max_per_day = 5;
		public static Dictionary <string,DateTime> minting = new Dictionary <string,DateTime> ();

		// Motion
		public static bool is_motion_active;
		public static bool is_vote_active;
		public static bool is_vote_public;

		public static Dictionary <string,string> motion = new Dictionary <string,string> ();
		public static Dictionary <string,bool> votes = new Dictionary <string,bool> ();

		// Internal-Groups
		public static List <string> Admins;
		public static List <string> Mods;

		// Mod Storage Location
		public static string save = System.IO.Path.Combine(
			System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
			"ArchMod"
		);

		// Class Constructor
		static ArchModCommands() {
			silent_enable_mod();
		}

		// TODO: Currency->SP Exchange, Casino?

		/*
			COMMANDS
		*/

		/* MINT */
		[ChatCommand("Mint Command", ChatAuthorizationLevel.User)]
		public static void mint (User user, string param = "", string arg = "") {
			Currency astr = EconomyManager.Currency.GetCurrency("Astrum");

			if (minting.ContainsKey(user.Name)) {
				DateTime last = minting[user.Name];
				DateTime now = new DateTime();

				TimeSpan t = last - now;

				if (t.TotalHours < 24) {
					send_pm(
						"<color=#FF4444>ERROR: You may only access the Mint Pool once per day.</color>",
						user.Player, ChatCategory.Default, DefaultChatTags.Government
					);

					return;
				}
			} 

			ItemStack itemStack = user.Inventory.NonEmptyStacks.Where(
				stack => stack.Item.FriendlyName == "Gold Ore"
			).FirstOrDefault();

			if (itemStack != null) {
				int items = 0;
				for (var i = 0; i < mint_max_per_day; i++) {
					bool worked = user.Inventory.TryRemoveItem<GoldOreItem>(user);
					if (! worked) {
						// failed
						break;
					}
					items++;
				}

				float amount = (float) items;
				float val = amount * 10f;

				// checks
				if (items == 0) {
					send_pm(
						"<color=#FF4444>ERROR: You must mint something, get some GoldOre.</color>",
						user.Player, ChatCategory.Default, DefaultChatTags.Government
					);
				} else if (mint_pool < val) {
					InventoryChangeSet changes = new InventoryChangeSet(user.Inventory, user);
					changes.AddItems<GoldOreItem>(items);
					send_pm(
						"<color=#FF4444>ERROR: Mint Pool does not have enough " + astr.UILink() +
						" to cover that, (PoolBalance:" + mint_pool + ").</color>",
						user.Player, ChatCategory.Default, DefaultChatTags.Government
					);
				} else if (astr.GetAccount(" _Treasury").Val < val) {
					InventoryChangeSet changes = new InventoryChangeSet(user.Inventory, user);
					changes.AddItems<GoldOreItem>(items);
					send_pm(
						"<color=#FF4444>ERROR: Treasury does not have enough " + astr.UILink() + " to cover that.</color>",
						user.Player, ChatCategory.Default, DefaultChatTags.Government
					);
				} else {
					// award
					mint_pool -= val;
					astr.GetAccount(" _Treasury").Val -= val;

					minting[ user.Name ] = new DateTime();
					astr.GetAccount(user.Name).Val += val;
					send_msg(
						"<color=#44FF44>NOTICE: " + user.Player.FriendlyName + " has converted " +
						val + " " + astr.UILink() + " from the Mint pool, (PoolBalance:" + mint_pool + ").</color>",
						ChatCategory.Default, DefaultChatTags.Government
					);
				}
			} else {
				// not enough
				send_pm(
					"<color=#FF4444>ERROR: You must mint <i>something</i>, go get some GoldOre.</color>",
					user.Player, ChatCategory.Default, DefaultChatTags.Government
				);
				return;
			}

		}

		/* MOTION */
		[ChatCommand("Motion Command", ChatAuthorizationLevel.Admin)]
		public static void Motion (User user, string param = "", string val = "") {

			// motion $param
			switch (param) {

				//
				case "create":
					if (is_motion_active == true) {
						send_pm(
							"<color=#FF4444>ERROR: Another motion is already on the table.</color>",
							user.Player, ChatCategory.Default, DefaultChatTags.Government
						);
						return;
					}

					is_motion_active = true;
					motion.Clear();
					votes.Clear();

					motion["question"] = val;
					motion["creator"] = user.Name;
					send_pm(
						"<color=#44FF44>Motion Created. Now do /motion summary to add/update the long description.</color>",
						user.Player, ChatCategory.Default, DefaultChatTags.Government
					);
				break;

				case "question":
					if (val != "") {
						if (motion["creator"] != user.Name) {
							send_pm(
								"<color=#FF4444>ERROR: You cannot modify this motion, as it is not yours.</color>",
								user.Player, ChatCategory.Default, DefaultChatTags.Government
							);
							return;
						}
						motion["question"] = val;
					} else {
						send_msg(
							"<color=#FFCC44>" + motion["question"] + "</color>"
							, ChatCategory.Default, DefaultChatTags.Government
						);
					}
				break;

				case "summary":
					if (val != "") {
						if (motion["creator"] != user.Name) {
							send_pm(
								"<color=#FF4444>ERROR: You cannot modify this motion, as it is not yours.</color>",
								user.Player, ChatCategory.Default, DefaultChatTags.Government
							);
							return;
						}
						motion["summary"] = val;
						send_pm(
							"<color=#4444FF>Motion Updated. Now do /motion propose to send it live.</color>",
							user.Player, ChatCategory.Default, DefaultChatTags.Government
						);
					} else {
						send_msg(
							"<color=#44FF44>" + motion["summary"] + "</color>"
							, ChatCategory.Default, DefaultChatTags.Government
						);
					}
				break;

				case "propose":
					if (motion["creator"] != user.Name) {
						send_pm(
							"<color=#FF4444>ERROR: You cannot modify this motion, as it is not yours.</color>",
							user.Player, ChatCategory.Default, DefaultChatTags.Government
						);
						return;
					}

					send_msg(
						"<color=#EE44EE> * </color><color=#4444FF>" + user.Player.FriendlyName +
						" proposes the following motion:</color><br><br>" +
						"<color=#FFCC44>" + motion["question"] + "</color><br>" +
						"<color=#44FF44>" + motion["summary"] + "</color><br>"
						, ChatCategory.Default, DefaultChatTags.Government
					);

					System.Threading.Thread.Sleep(100);

					send_msg(
						"<br><color=#EE44EE> * </color><color=#4444FF>The chair auto-recognizes " + user.Player.FriendlyName +
						" as speaker, with the privilege of arguing in favor of their motion first--" +
						" they have 10 minutes.</color>"
						, ChatCategory.Default, DefaultChatTags.Government
					);
				break;

				case "pass":
					is_motion_active = false;
					is_vote_active = false;
					is_vote_public = false;
					send_msg(
						"<color=#44FF44>PASS: Motion '" + motion["question"] +
						"' has been passed.<br>Please use the Eco LawUI to propose and then /enact it into law.</color>",
						ChatCategory.Default, DefaultChatTags.Government
					);
					motion.Clear();
					votes.Clear();
				break;

				case "reject":
					is_motion_active = false;
					is_vote_active = false;
					is_vote_public = false;
					send_msg(
						"<color=#FF4444>REJECT: Motion '" + motion["question"] + "' has been rejected.</color>",
						ChatCategory.Default, DefaultChatTags.Government
					);
					motion.Clear();
					votes.Clear();
				break;

				case "open-vote":
					if (is_vote_active == true) {
						send_pm(
							"<color=#FF4444>ERROR: Another vote is already in progress.</color>",
							user.Player, ChatCategory.Default, DefaultChatTags.Government
						);
						return;
					}
					is_vote_active = true;
					is_vote_public = true;
					votes.Clear();

					send_msg(
						"<color=#44FF44>OPEN-VOTE: We shall now commence voting on the question: '" + motion["question"] +
						"', please vote with /yea and /nay respectively.</color>",
						ChatCategory.Default, DefaultChatTags.Government
					);
				break;

				case "closed-vote":
					if (is_vote_active == true) {
						send_pm(
							"<color=#FF4444>ERROR: Another vote is already in progress.</color>",
							user.Player, ChatCategory.Default, DefaultChatTags.Government
						);
						return;
					}
					is_vote_active = true;
					is_vote_public = false;
					votes.Clear();

					send_msg(
						"<color=#44FF44>CLOSED-VOTE: We shall now commence voting on the question: '" + motion["question"] +
						"', please vote with /yea and /nay respectively.</color>",
						ChatCategory.Default, DefaultChatTags.Government
					);
				break;

				default:
					send_pm(
						"<color=#FF4444>ERROR: There is no command action called '" + param +
						"'. Please choose from [create|question|summary|propose|pass|reject|open-vote|closed-vote]</color>",
						user.Player, ChatCategory.Default, DefaultChatTags.Government
					);
				break;
			}
		}

		/* YEA */
		[ChatCommand("Vote YEA", ChatAuthorizationLevel.User)]
		public static void yea (User user) {
			if (! is_vote_active) {
				send_pm(
					"<color=#FF4444>ERROR: There is no active vote in progress, " +
					"please wait until there is before attempting to vote.</color>",
					user.Player, ChatCategory.Info, DefaultChatTags.Government
				);
				return;
			}
			if (is_vote_public || is_admin_or_mod(user)) {
				votes[ user.Name ] = true;

				int yea = votes.Count(kvp => kvp.Value.Equals(true));
				int nay = votes.Count(kvp => kvp.Value.Equals(false));

				send_msg(
					"<color=#44AAFF> * </color> All in favor: <color=#44FF44>" + yea + "</color> " + 
					"| Opposed: <color=#FF4444>" + nay + "</color>"
					, ChatCategory.Info, DefaultChatTags.Government
				);
			} else {
				send_pm(
					"<color=#FF4444>ERROR: You are not allowed to vote on this question.</color>",
					user.Player, ChatCategory.Info, DefaultChatTags.Government
				);
			}
		}

		/* NAY */
		[ChatCommand("Vote NAY", ChatAuthorizationLevel.User)]
		public static void nay (User user) {
			if (! is_vote_active) {
				send_pm(
					"<color=#FF4444>ERROR: There is no active vote in progress, " +
					"please wait until there is before attempting to vote.</color>",
					user.Player, ChatCategory.Info, DefaultChatTags.Government
				);
				return;
			}
			if (is_vote_public || is_admin_or_mod(user)) {
				votes[ user.Name ] = false;

				int yea = votes.Count(kvp => kvp.Value.Equals(true));
				int nay = votes.Count(kvp => kvp.Value.Equals(false));

				send_msg(
					"<color=#44AAFF> * </color> All in favor: <color=#44FF44>" + yea + "</color> " + 
					"| Opposed: <color=#FF4444>" + nay + "</color>"
					, ChatCategory.Info, DefaultChatTags.Government
				);

			} else {
				send_pm(
					"<color=#FF4444>ERROR: You are not allowed to vote on this question.</color>",
					user.Player, ChatCategory.Info, DefaultChatTags.Government
				);
			}
		}

		/* ENACT */
		[ChatCommand("Enact Command", ChatAuthorizationLevel.Admin)]
		public static void enact (User user, string guid) {
			if (guid != "") {
				Law law = Legislation.Laws.GetLawByGuid(guid);
				if (law is Law) {
					Legislation.Laws.EnactLaw(law);
				} else {
					send_pm(
						"<color=#FF4444>ERROR: Invalid LAWID, please double check it exists.</color>",
						user.Player, ChatCategory.Info, DefaultChatTags.Government
					);
				}
			} else {
				send_pm(
					"<color=#FF4444>ERROR: This command takes a string LAWID as an argument.</color>",
					user.Player, ChatCategory.Info, DefaultChatTags.Government
				);
			}
		}

		/* REPEAL */
		[ChatCommand("Repeal Command", ChatAuthorizationLevel.Admin)]
		public static void repeal (User user, string guid) {
			if (guid != "") {
				Law law = Legislation.Laws.GetLawByGuid(guid);
				if (law is Law) {
					Legislation.Laws.RemoveLaw(Guid.Parse(guid));
				} else {
					send_pm(
						"<color=#FF4444>ERROR: Invalid LAWID, please double check it exists.</color>",
						user.Player, ChatCategory.Info, DefaultChatTags.Government
					);
				}
			} else {
				send_pm(
					"<color=#FF4444>ERROR: This command takes a string LAWID as an argument.</color>",
					user.Player, ChatCategory.Info, DefaultChatTags.Government
				);
			}
		}

		/* ALLOCATE */
		[ChatCommand("Allocate Command", ChatAuthorizationLevel.Admin)]
		public static void allocate (User user, User target, int amount, string reason = "", string currency = "Astrum") {
			float val = amount;
			Currency astr = EconomyManager.Currency.GetCurrency(currency);
			if (astr.GetAccount(" _Treasury").Val < val) {
				send_pm(
					"<color=#FF4444>ERROR: Treasury does not have enough " + astr.UILink() + " to cover that.</color>",
					user.Player, ChatCategory.Default, DefaultChatTags.Government
				);
			} else {
				astr.GetAccount(target.Name).Val += val;
				astr.GetAccount(" _Treasury").Val -= val;
				send_msg(
					"<color=#44FF44>NOTICE: " + target.Player.FriendlyName + " has been allocated " +
					val + " " + astr.UILink() + ". Reason: " + reason + ".</color>",
					ChatCategory.Default, DefaultChatTags.Government
				);
			}
		}

		/* DONATE */
		[ChatCommand("Donate Command", ChatAuthorizationLevel.User)]
		public static void donate (User user, int amount, string reason = "") {
			float val = amount;
			Currency astr = EconomyManager.Currency.GetCurrency("Astrum");
			if (astr.GetAccount(user.Name).Val < val) {
				send_pm(
					"<color=#FF4444>ERROR: You do not have enough " + astr.UILink() + " to cover that.</color>",
					user.Player, ChatCategory.Default, DefaultChatTags.Government
				);
			} else {
				astr.GetAccount(user.Name).Val -= val;
				astr.GetAccount(" _Treasury").Val += val;
				send_msg(
					"<color=#44FF44>NOTICE: " + user.Player.FriendlyName + " donated " +
					val + " " + astr.UILink() + " to the Crown. Reason: " + reason + "</color>",
					ChatCategory.Default, DefaultChatTags.Government
				);
				System.Threading.Thread.Sleep(100);
				send_pm(
					"<color=#EE44CC>Thank you for your donation. <3</color>",
					user.Player, ChatCategory.Default, DefaultChatTags.Government
				);
			}
		}

		/* FINE */
		[ChatCommand("Fine Command", ChatAuthorizationLevel.Admin)]
		public static void fine (User user, User target, int amount, string reason = "") {
			float val = amount;
			Currency astr = EconomyManager.Currency.GetCurrency("Astrum");
			if (astr.GetAccount(target.Name).Val < val) {
				send_pm(
					"<color=#EE4444>ERROR: " + target.Player.FriendlyName +
					" does not have enough " + astr.UILink() + " to cover that.</color>",
					user.Player, ChatCategory.Default, DefaultChatTags.Government
				);
			} else {
				astr.GetAccount(target.Name).Val -= val;
				astr.GetAccount(" _Treasury").Val += val;
				send_msg(
					"<color=#EE4444>NOTICE: " + target.Player.FriendlyName + " was " +
					"</color><color=#FFCC44>fined</color><color=#44EE44> " +
					val + " " + astr.UILink() + ", Reason: " + reason + "</color>",
					ChatCategory.Default, DefaultChatTags.Government
				);
			}
		}

		/* BCAST */
		[ChatCommand("Broadcast Command", ChatAuthorizationLevel.Admin)]
		public static void bcast (User user, int level = 0, string msg = "") {
			if (level > 2)
				level = 2;

			switch(level) {
				//
				case 2:
					send_msg(
						" <color=#DD2222>*</color> <color=#FF4444>ALERT: " + msg + "</color>",
						ChatCategory.Default, DefaultChatTags.Notifications
					);
				break;

				case 1:
					send_msg(
						" <color=#DDAA22>*</color> <color=#FFCC44>NOTICE: " + msg + "</color>",
						ChatCategory.Default, DefaultChatTags.Notifications
					);
				break;

				case 0:
					send_msg(
						" <color=#22DD22>*</color> <color=#44FF44>INFO: " + msg + "</color>",
						ChatCategory.Default, DefaultChatTags.Notifications
					);
				break;
			}
		}

		/* MOTD */
		[ChatCommand("Message-Of-The-Day Command", ChatAuthorizationLevel.Admin)]
		public static void motd (User user, string param = "", string val = "") {

			// motd $param
			switch(param) {

				//
				case "disable":
					disable_motd(user.Player);
				break;

				//
				case "add":
					messages.Add(val);
					send_pm("Message added: " + val, user.Player,
						ChatCategory.Info, DefaultChatTags.Notifications);
					save_messages();
				break;

				//
				case "remove":
					send_pm("Removed: " + messages[int.Parse(val)], user.Player,
						ChatCategory.Info, DefaultChatTags.Notifications);
					messages.RemoveAt(int.Parse(val));
					save_messages();
				break;

				//
				case "list":
					int tempx = 0;
					foreach (string tempstring in messages) {
						send_pm(tempx + " " + tempstring, user.Player,
							ChatCategory.Info, DefaultChatTags.Notifications);
						tempx += 1;
					}
				break;

				//
				case "send":
					send_motd(null);
				break;

				//
				default:
					send_pm("Please provide an argument to this command: [add|remove|list|send|disable]",
						user.Player, ChatCategory.Default, DefaultChatTags.Notifications);
				break;
			}
		}

		/* INFO */
		[ChatCommand("Info Command", ChatAuthorizationLevel.User)]
		public static void info (User user) {
			string text = load_file("info.txt");
			if (text != "")
				send_pm(text, user.Player, ChatCategory.Default, DefaultChatTags.Notifications);
		}

		/* RULES */
		[ChatCommand("Rules Command", ChatAuthorizationLevel.User)]
		public static void rules (User user) {
			string text = load_file("rules.txt");
			if (text != "")
				send_pm(text, user.Player, ChatCategory.Default, DefaultChatTags.Notifications);
		}

		// *not working*
		/* NEED */
		[ChatCommand("Need Command *(Not working yet, don't run this.)*", ChatAuthorizationLevel.User)]
		public static void need (User user, string skillname) {
			string result = "";

			foreach (User u in UserManager.Users) {
				string online = "";

				if (u.LoggedIn)
					online = "(<color=#33FF33>online</color>)";

				foreach (Skill skill in u.Skillset.Skills) {
					bool match = Regex.IsMatch(skill.FriendlyName.ToLower(), skillname.ToLower());
					if (match) {
						result += u.Player.FriendlyName + " (" + skill.Level + ") " + online + "\n";
					}
				}
			}

			if (result != "") {
				send_pm(result, user.Player, ChatCategory.Info, DefaultChatTags.Notifications);
			} else {
				send_pm("No matching players found.", user.Player, ChatCategory.Info, DefaultChatTags.Notifications);
			}
		}

		/*
			BASE METHODS
		*/

		private static void send_pm (string text, Player player, ChatCategory Category, DefaultChatTags Tags) {
			System.FormattableString s = $"{text}";
			ChatManager.ServerMessageToPlayer(s, player.User, false, Tags, Category);
		}

		private static void send_msg (string text, ChatCategory Category, DefaultChatTags Tags) {
			System.FormattableString s = $"{text}";
			ChatManager.ServerMessageToAll(s, false, Tags, Category);
		}

		private static void silent_enable_mod () {
			if (!Directory.Exists(save)) {
				Directory.CreateDirectory(save);
			}
			// loading the saved messages
			load_messages();

			motd_timer = new Timer(send_motd, null, 0, motd_interval);
			groups_sync_timer = new Timer(load_groups, null, 0, groups_sync_interval);
			notify_sync_timer = new Timer(do_notify, null, 0, notify_sync_interval);
		}

		private static void disable_motd (Player player) {
			string text = "MOTD Deactivated";
			motd_timer.Dispose();
			send_pm(text, player, ChatCategory.Info, DefaultChatTags.Notifications);
		}

		private static void rotate_messages () {
			string message = messages.First();
			messages.RemoveAt(0);
			messages.Add(message);
		}

		private static void send_motd (object sender) {
			string message = messages.First();
			send_msg(message, ChatCategory.Default, DefaultChatTags.Notifications);
			rotate_messages();
		}

		private static void save_messages () {
			write_file("motd.txt", messages);
		}

		private static void load_messages () {
			string content = load_file("motd.txt");
			messages = content.Split('\n').ToList();
		}

/*
        private static void write_dict_file (string filename, ) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(save + '/' + filename);
            bf.Serialize(file, messages);
            file.Close();
        }

        private static void read_dict_file (string filename) {
            if (File.Exists(save + "/SavedMessages.arc")) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(save + '/' + filename, FileMode.Open);
                messages = (List<string>) bf.Deserialize(file);
                file.Close();
            }
        }
*/

		private static string load_file (string filename) {
			if (!File.Exists(save + '/' + filename))
			return string.Empty;

			var content = string.Empty;
			using (StreamReader file = new StreamReader(save + '/' + filename)) {
				content = file.ReadToEnd();
			}
			return content;
		}

		private static void write_file (string filename, List<string> contents) {
			string[] lines = contents.ToArray();
			File.WriteAllText(save + '/' + filename, string.Join("\n", lines));
		}

		private static void truncate_file (string filename) {
			if (!File.Exists(save + '/' + filename))
				return;

			FileStream file = File.Open(save + '/' + filename, FileMode.Open);
			if (file.Length != 0) {
				file.SetLength(0);
			}
			file.Close();
		}

		private static void load_groups (object sender) {
			string atext = load_file("admins.txt");
			string mtext = load_file("mods.txt");

			Admins = atext.Split('\n').ToList();
			Mods = mtext.Split('\n').ToList();
		}

		private static void do_notify (object sender) {
			string text = load_file("notification.fifo");
			if (text != string.Empty) {
				truncate_file("notification.fifo");
			
				send_msg(
					" <color=#DDAA22>*</color> <color=#FFCC44>NOTICE: " + text + "</color>",
					ChatCategory.Default, DefaultChatTags.Notifications
				);
			}
		}

		private static bool is_admin_or_mod (User user) {
			load_groups(null);
			if (Admins.Contains(user.Player.FriendlyName) || Mods.Contains(user.Player.FriendlyName))
				return true;
			return false;
		}
	}

	/* Partial OVERRIDE */
	partial class AdminCommands : IChatCommandHandler {

		// leader override
		[ChatCommand("Leader Command", ChatAuthorizationLevel.Admin)]
		public static void leader (User user, User target = null) {
			if (user.Player.FriendlyName == "Archpoet") {
				Election e = new Election();
				if (target != null) {
					e.ForceLeader(target.Name, "");
					send_msg(
						" <color=#DD2222>*</color> <color=#EE44CC>" +
						target.Player.FriendlyName + " is now the</color> " +
						"<color=#FFCC44>Regent</color> <color=#EE44CC>of the Realm.</color>",
						ChatCategory.Default, DefaultChatTags.Government
					);
					System.Threading.Thread.Sleep(100);
					send_pm(
						" <color=#22DD22>*</color> <color=#44CCEE>You have been made Regent of the Realm.</color>",
						target.Player, ChatCategory.Default, DefaultChatTags.Government
					);
				} else {
					e.ForceLeader(user.Name, "");
					send_msg(
						" <color=#DD2222>*</color> <color=#EE44CC>The</color> " +
						"<color=#FFCC44>King</color> <color=#EE44CC>has returned. Long live the</color> " +
						"<color=#FFCC44>King!</color>",
						ChatCategory.Default, DefaultChatTags.Government
					);
					System.Threading.Thread.Sleep(100);
					send_pm(
						" <color=#22DD22>*</color> <color=#44CCEE>Welcome back, Your Majesty. <3</color>",
						user.Player, ChatCategory.Default, DefaultChatTags.Government
					);
				}
			} else {
				send_pm(
					" <color=#DD2222>*</color> <color=#FF4444>Yours is not the Royal Prerogative.</color>",
					user.Player, ChatCategory.Default, DefaultChatTags.Government
				);
			}
		}

		private static void send_pm (string text, Player player, ChatCategory Category, DefaultChatTags Tags) {
			System.FormattableString s = $"{text}";
			ChatManager.ServerMessageToPlayer(s, player.User, false, Tags, Category);
		}

		private static void send_msg (string text, ChatCategory Category, DefaultChatTags Tags) {
			System.FormattableString s = $"{text}";
			ChatManager.ServerMessageToAll(s, false, Tags, Category);
		}

	}

}
