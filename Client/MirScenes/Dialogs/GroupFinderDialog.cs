﻿using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirObjects;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C = ClientPackets;

namespace Client.MirScenes.Dialogs
{
    public sealed class GroupFinderDialog : MirImageControl
    {
        public static long SearchTime;

        public MirImageControl TitleLabel;
        public MirLabel PageLabel;
        public MirButton CloseButton, CreateButton, RefreshButton, NextButton, BackButton;

        public List<GroupFinderDetail> GroupFinderDetails = new List<GroupFinderDetail>();

        public int Page, PageCount;
        public static GroupFinderDialogRow Selected;
        public GroupFinderDialogRow[] Rows = new GroupFinderDialogRow[10];

        public GroupFinderDialog()
        {
            Index = 782;
            Library = Libraries.Prguse3;
            Sort = true;
            Movable = true;

            Location = Center;

            TitleLabel = new MirImageControl
            {
                Index = 24,
                Library = Libraries.Title,
                Location = new Point(18, 9),
                Parent = this
            };

            CloseButton = new MirButton
            {
                HoverIndex = 361,
                Index = 360,
                Location = new Point(662, 3),
                Library = Libraries.Prguse2,
                Parent = this,
                PressedIndex = 362,
                Sound = SoundList.ButtonA,
            };

            CloseButton.Click += (o, e) => Hide();

            CreateButton = new MirButton
            {
                Index = 199,
                HoverIndex = 200,
                PressedIndex = 201,
                Location = new Point(18, 444),
                Library = Libraries.Prguse3,
                Parent = this,
                Sound = SoundList.ButtonA,
            };

            CreateButton.Click += (o, e) =>
            {
                if (GameScene.Scene.GroupFinderFormDialog.Visible == true)
                {
                    GameScene.Scene.GroupFinderFormDialog.Hide();
                }
                else
                {
                    GameScene.Scene.GroupFinderFormDialog.Show();
                    GameScene.Scene.GroupFinderFormDialog.TitleTextBox.SetFocus();
                }
            };

            RefreshButton = new MirButton
            {
                Index = 663,
                HoverIndex = 664,
                PressedIndex = 665,
                Location = new Point(710, 440),
                Library = Libraries.Prguse,
                Parent = this,
                Sound = SoundList.ButtonA,
            };

            RefreshButton.Click += (o, e) =>
            {
                if (CMain.Time < SearchTime)
                {
                    GameScene.Scene.ChatDialog.ReceiveChat(string.Format("You can search again after {0} seconds.", Math.Ceiling((SearchTime - CMain.Time) / 1000D)), ChatType.System);
                    return;
                }
                SearchTime = CMain.Time + Globals.GroupFinderRefreshDelay;

                Network.Enqueue(new C.GroupFinderRefresh());
            };

            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new GroupFinderDialogRow
                {
                    Location = new Point(32, 76 + i * 33),
                    Parent = this
                };
                Rows[i].Click += (o, e) =>
                {
                    Selected = (GroupFinderDialogRow)o;
                    UpdateInterface();
                };
            }

            PageLabel = new MirLabel
            {
                Location = new Point(300, 444),
                Size = new Size(70, 18),
                DrawFormat = TextFormatFlags.HorizontalCenter,
                Parent = this,
                NotControl = true,
                Text = "0/0",
            };

            BackButton = new MirButton
            {
                Index = 398,
                Location = new Point(282, 444),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 399,
                Sound = SoundList.ButtonA,
            };
            BackButton.Click += (o, e) =>
            {
                if (Page <= 0) return;

                Page--;
                UpdateInterface();
            };

            NextButton = new MirButton
            {
                Index = 396,
                Location = new Point(378, 444),
                Library = Libraries.Prguse,
                Parent = this,
                PressedIndex = 397,
                Sound = SoundList.ButtonA,
            };

            NextButton.Click += (o, e) =>
            {
                if (Page >= PageCount - 1) return;
                if (Page < (GroupFinderDetails.Count - 1) / 10)
                {
                    Page++;
                    UpdateInterface();
                    return;
                }

                Network.Enqueue(new C.GroupFinderPage { Page = Page + 1 });

            };
            UpdateInterface();
        }
        public void UpdateInterface()
        {

            PageLabel.Text = string.Format("{0}/{1}", Page + 1, PageCount);

            for (int i = 0; i < 10; i++)
                if (i + Page * 10 >= GroupFinderDetails.Count)
                {
                    Rows[i].Clear();
                    if (Rows[i] == Selected) Selected = null;
                }
                else
                {
                    if (Rows[i] == Selected && Selected.Detail != GroupFinderDetails[i + Page * 10])
                    {
                        Selected.Border = false;
                        Selected = null;
                    }
                    Rows[i].Update(GroupFinderDetails[i + Page * 10]);
                }

            for (int i = 0; i < Rows.Length; i++)
                Rows[i].Border = Rows[i] == Selected;

        }
        public void Hide()
        {
            if (!Visible) return;
            if (GameScene.Scene.GroupFinderFormDialog.Visible)
                GameScene.Scene.GroupFinderFormDialog.Hide();
            Visible = false;
        }
        public void Show(bool showCreateButton)
        {
            if (Visible) return;
            Visible = true;
            CreateButton.Visible = showCreateButton;
            Network.Enqueue(new C.GroupFinderRefresh());

        }
        public override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            switch (e.KeyChar)
            {
                case (char)Keys.Escape:
                    e.Handled = true;
                    Hide();
                    break;
            }
        }
    }
    
    public sealed class GroupFinderDialogRow : MirControl
    {
        private readonly bool ShowBorders = false;

        public GroupFinderDetail Detail;
        public MirLabel MinimumLevelLabel, PlayerNameLabel, TitleLabel, DescriptionLabel, CreatedLabel, PlayerLimitLabel;
        public MirButton JoinGroupButton, CloseButton;

        public GroupFinderDialogRow()
        {
            Sound = SoundList.ButtonA;
            Size = new Size(750, 30);

            MinimumLevelLabel = new MirLabel
            {
                Size = new Size(60, 30),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                NotControl = true,
                Border = ShowBorders,
                BorderColour = Color.Red
            };

            PlayerNameLabel = new MirLabel
            {
                Size = new Size(88, 30),
                Location = new Point(62, 0),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                NotControl = true,
                Border = ShowBorders,
                BorderColour = Color.Blue
            };

            TitleLabel = new MirLabel
            {
                Size = new Size(105, 30),
                Location = new Point(153),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                NotControl = true,
                Border = ShowBorders,
                BorderColour = Color.Green
            };

            DescriptionLabel = new MirLabel
            {
                Size = new Size(177, 30),
                Location = new Point(261),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                NotControl = true,
                Border = ShowBorders,
                BorderColour = Color.Red
            };

            CreatedLabel = new MirLabel
            {
                Size = new Size(116, 30),
                Location = new Point(442),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                NotControl = true,
                Border = ShowBorders,
                BorderColour = Color.Green
            };

            PlayerLimitLabel = new MirLabel
            {
                Size = new Size(111, 30),
                Location = new Point(562),
                DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter,
                Parent = this,
                NotControl = true,
                Border = ShowBorders,
                BorderColour = Color.Yellow
            };

            JoinGroupButton = new MirButton
            {

                Index = 214,
                HoverIndex = 215,
                PressedIndex = 216,
                Location = new Point(685, 10),
                Library = Libraries.Prguse3,
                Parent = this,
                Sound = SoundList.ButtonA
            };

            CloseButton = new MirButton
            {

                Index = 196,
                HoverIndex = 197,
                PressedIndex = 198,
                Location = new Point(685, 10),
                Library = Libraries.Prguse3,
                Parent = this,
                Sound = SoundList.ButtonA
            };

            JoinGroupButton.Click += JoinGroupButton_Click;
            CloseButton.Click += CloseButton_Click;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Network.Enqueue(new C.DeleteGroupFinder { Name = GameScene.User.Name });

            Network.Enqueue(new C.GroupFinderRefresh());

            GameScene.Scene.GroupFinderDialog.Hide();

        }

        private void JoinGroupButton_Click(object sender, EventArgs e)
        {
            GameScene.Scene.ChatDialog.ReceiveChat($"Sending request to join group to {PlayerNameLabel.Text}.", ChatType.System);

            Network.Enqueue(new C.JoinMember { Name = PlayerNameLabel.Text });

            Network.Enqueue(new C.GroupFinderRefresh());

            GameScene.Scene.GroupFinderDialog.Hide();
        }

        public void Clear()
        {
            Visible = false;
            MinimumLevelLabel.Text = string.Empty;
            PlayerNameLabel.Text = string.Empty;
            TitleLabel.Text = string.Empty;
            DescriptionLabel.Text = string.Empty;
            CreatedLabel.Text = string.Empty;
            PlayerLimitLabel.Text = string.Empty;
        }
        public void Update(GroupFinderDetail listing)
        {
            int GroupMemberCount = listing.GroupMemberCount > 0 ? listing.GroupMemberCount : 1;
            MinimumLevelLabel.Text = listing.MinimumLevel.ToString();
            PlayerNameLabel.Text = listing.PlayerName;
            TitleLabel.Text = listing.Title;
            DescriptionLabel.Text = listing.Description;
            CreatedLabel.Text = listing.Created.ToString("dd/MM/yy H:mm:ss");
            PlayerLimitLabel.Text = GroupMemberCount > 0 ? $"{GroupMemberCount}/{listing.PlayerLimit}" : "OFFLINE";
            JoinGroupButton.Visible = listing.PlayerName != GameScene.User.Name && GroupMemberCount < listing.PlayerLimit && !listing.GroupMemberNames.Any(x => x == GameScene.User.Name);
            CloseButton.Visible = listing.PlayerName == GameScene.User.Name;
            Visible = true;
        }
    }
}
