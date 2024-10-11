namespace DiamondShop.Domain.Common.Carts
{
    public class CartModelValidation
    {
        public bool IsOrderValid { get => InvalidItemIndex.Length == 0 && UnavailableItemIndex.Length == 0; }
        public int[] InvalidItemIndex { get; set; } = new int[] { };
        public int[] UnavailableItemIndex { get; set; } = new int[] { };
    }

}
