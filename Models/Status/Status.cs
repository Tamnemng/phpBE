public enum ProductStatus {

    /// <summary>
    /// Product is created for set the status
    /// </summary>
    New,

    /// <summary>
    /// Product is currently for sale
    /// </summary>
    InStock,

    /// <summary>
    /// Product is temporarily out of stock and awaiting new inventory
    /// </summary>
    Pending,

    /// <summary>
    /// Product is completely out of stock
    /// </summary>
    OutOfStock
}