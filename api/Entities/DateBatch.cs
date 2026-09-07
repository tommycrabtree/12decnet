namespace api.Entities;

public class DateBatch
{
    public int Id { get; set; }

    // This DateBatch belongs to the ReplenishmentSession whose primary key is ReplenishmentSessionId
    public int ReplenishmentSessionId { get; set; }

    // This DateBatch belongs to the Product whose primary key is ProductId
    public int ProductId { get; set; }

    public int RequestUnits { get; set; }
    public int DateUnits { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }
    public int? FiftyPercentOffUnits { get; set; }
    public int? DonationUnits { get; set; }

    // These navigation / reference properties allow EF Core and my C# code to navigate related entities.

    // For example, I can write 'dateBatch.Product.Name'
    // instead of having to separately retrieve the Product using 'dateBatch.ProductId'

    // The same is true for dateBatch.ReplenishmentSession.DateBatches,
    // which lets me navigate through the object graph.

    // A database schema describes how data is structured and related in the database.
    // An object graph describes how objects are structured and related in memory in my application.

    // The object graph lets me go beyond the DateBatch's own properties and follow its references to related objects.

    // An array can contain objects, but an object graph is the network of objects
    // that can be reached by following references from one object to another.

    // Because dateBatch.Product is a reference to a Product object, I can keep navigating:
    // For example, dateBatch.Product.Name says:
    // "Start with this DateBatch → go to its Product → give me the Product's Name."

    // Similarly, dateBatch.ReplenishmentSession.DateBatches says:
    // "Start with this DateBatch → go to its ReplenishmentSession → give me that session's collection of DateBatches."

    // I'm essentially walking around a network of connected objects.  That's an object graph.

    // The navigation properties aren't what create the foreign-key relationship;
    // they're what allow me to navigate the relationship through the C# object model.
    public Product Product { get; set; } = null!;
    public ReplenishmentSession ReplenishmentSession { get; set; } = null!;
}
