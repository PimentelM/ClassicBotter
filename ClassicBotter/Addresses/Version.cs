using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassicBotter.Addresses
{
    public class Version
    {
        public static void SetAddresses()
        {
            Player.Id = 0x05C684C;
            Player.Flags = 0x05C67D8;
            Player.X = 0x05D16F0;
            Player.Y = 0x05D16EC;
            Player.Z = 0x05D16E8;
            Player.Experience = 0x05C6840;
            Player.Level = 0x05C683C;
            Player.MagicLevel = 0x05C6838;
            Player.gotoY = 0x05C688C;
            Player.gotoX = 0x05C688C + 4;
            Player.gotoZ = 0x05C688C - 4;
            Player.Health = 0x05C6848;
            Player.HealthMax = 0x05C6844;
            Player.Mana = 0x05C682C;
            Player.ManaMax = 0x05C6828;
            Player.Cap = 0x05C6820;
            Player.TargetId = 0x05C681C;
            Player.LastRightClickId = 0x071C630;
            Player.LastRightClickCount = 0x071C634;
            Player.LastLeftClickId = 0x071C624;
            Player.LastLeftClickCount = 0x071C628;
            Player.SlotNeck = 0x05CED6C;
            Player.SlotRightHand = 0x05CED90;
            Player.SlotLeftHand = 0x05CED9C;
            Player.SlotAmmo = 0x05CEDCC;
            Player.SlotRing = 0x05CEDC0;

            Battlelist.Start = 0x05C68B4;
            Battlelist.StepCreatures = 156;
            Battlelist.MaxCreatures = 250;
            Battlelist.End = Battlelist.Start + (Battlelist.StepCreatures * Battlelist.MaxCreatures);

            Creature.DistanceId = -4;
            Creature.DistanceName = 0;
            Creature.DistanceType = -1;
            Creature.DistanceX = 32;
            Creature.DistanceY = 36;
            Creature.DistanceZ = 40;
            Creature.DistanceIsWalking = 72;
            Creature.DistanceLightSize = 112;
            Creature.DistanceLightColor = 116;
            Creature.DistanceHPBar = 128;
            Creature.DistanceIsVisible = 136;

            Client.StatusbarText = 0x071DBE0;
            Client.StatusbarTime = 0x071DBDC;
            Client.Cursor = 0x071C5E8;
            Client.Connection = 0x071C588;
            Client.LastMessageName = 0x071DE08;
            Client.LastMessageString = 0x071DE30;
            Client.NameSpy1 = 0x04C650B;
            Client.NameSpy2 = 0x04C6515;
            Client.NameSpy1Default = 19573;
            Client.NameSpy2Default = 17013;
            Client.LevelSpyNop = 0x04B0900;
            Client.LevelSpyAbove = Client.LevelSpyNop - 4;
            Client.LevelSpyBelow = Client.LevelSpyNop + 4;
            Client.LevelSpyNopDefault = 49451;
            Client.LevelSpyAboveDefault = 7;
            Client.LevelSpyBelowDefault = 2;
            Client.LevelSpyMin = 0;
            Client.LevelSpyMax = 7;
            Client.LevelSpyZDefault = 7;
            Client.LightNop = 0x04BF94B;
            Client.LightAmount = Client.LightNop + 3;
            Client.LightNopDefault = 1406;
            Client.GuiPointer = 0x05D16B0;

            
            Client.LevelSpy1 = 0x04C7740;
            Client.LevelSpy2 = 0x04C7819;
            Client.LevelSpy3 = 0x04C7884;
            Client.LevelSpyPtr = 0x05CE514;
            Client.LevelSpyAdd2 = 0x25D8;

            /*
            Client.LevelSpy1 = 0x004E115A;
            Client.LevelSpy2 = 0x004E12E0;
            Client.LevelSpy3 = 0x004E125F;
            Client.LevelSpyPtr = 0x061B608;
            Client.LevelSpyAdd2 = 0x25D8;*/

            Container.Start = 0x05CEDD8;
            Container.StepContainer = 492;
            Container.MaxStack = 100;
            Container.StepSlot = 12;
            Container.MaxContainers = 16;
            Container.DistanceAmount = 56;// -4; //
            Container.DistanceVolume = 48;
            Container.DistanceItemId = 60;
            Container.DistanceItemCount = 64;
            Container.DistanceIsOpen = 0;
            Container.DistanceName = 16;//-36;
            Container.End = Container.Start + (Container.MaxContainers * Container.StepContainer);
        }
    }
}
