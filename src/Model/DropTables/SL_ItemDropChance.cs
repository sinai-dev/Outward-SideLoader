namespace SideLoader
{
    public class SL_ItemDropChance : SL_ItemDrop
    {
        public override string ToString()
                   => $"Dice: {DiceValue}, {base.ToString()}";

        public int DiceValue = 1;
    }
}
