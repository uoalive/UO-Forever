#region Header
//   Vorspire    _,-'/-'/  MessageContentEdit.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using Server.Engines.CentralGump;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace Server.Engines.CentralGump
{
	public class MessageContentEditGump : TextInputPanelGump<Message>
	{
		public bool UseConfirmDialog { get; set; }

        public MessageContentEditGump(
            PlayerMobile user, Gump parent = null, Message message = null, bool useConfirm = true)
			: base(
				user,
				parent,
				width: 420,
				height: 420,
				selected: message,
				emptyText: "No message selected.",
				title: "News Message Edit")
		{
			UseConfirmDialog = useConfirm;
		}

		protected override void Compile()
		{
			base.Compile();

			if (Selected != null)
			{
				Input = Selected.Content;
			}
		}

		public override SuperGump Refresh(bool recompile = false)
		{
			Selected.Content = Input;

			return base.Refresh(recompile);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			if (Selected != null)
			{
				if (User.AccessLevel >= CentralGump.Access)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Save Changes",
							subButton =>
							{
								Selected.Content = Input;
								Close();
							}));

					list.AppendEntry(
						new ListGumpEntry(
							"Clear Content",
							subButton =>
							{
								Selected.Content = Input = String.Empty;
								Refresh(true);
							}));
				}
			}

			base.CompileMenuOptions(list);
		}
	}
}