using XFramework.Domain.SharedKernel;

namespace XFramework.Domain.Inventory;

public static class CostingEngine
{
    public static Money CalculateAverageCost(Money currentTotalCost, Quantity currentBalance, Money incomingCost, Quantity incomingQuantity)
    {
        if (currentBalance.Value < 0 || incomingQuantity.Value < 0)
            throw new ArgumentException("Quantities cannot be negative.");
        
        if (currentBalance.Unit != incomingQuantity.Unit)
            throw new InvalidOperationException("Units must match for cost calculation.");
        
        if (currentTotalCost.Currency != incomingCost.Currency)
            throw new InvalidOperationException("Currencies must match for cost calculation.");

        var newTotalValue = currentTotalCost + (incomingCost * incomingQuantity.Value);
        var newTotalQuantity = currentBalance.Value + incomingQuantity.Value;

        if (newTotalQuantity == 0)
            return Money.Zero(currentTotalCost.Currency);

        var averageUnitCost = newTotalValue / newTotalQuantity;
        return averageUnitCost;
    }

    public static Money CalculateFifoCost(Queue<(Quantity Quantity, Money UnitCost)> fifoLayers, Quantity quantityToIssue)
    {
        if (quantityToIssue.Value <= 0)
            throw new ArgumentException("Quantity to issue must be positive.", nameof(quantityToIssue));

        var remainingQty = quantityToIssue.Value;
        var totalCost = Money.Zero(quantityToIssue.Unit == UnitOfMeasure.Piece ? Currency.IRR : Currency.IRR); // Will be set from first layer
        var firstLayer = true;

        var tempLayers = new Queue<(Quantity Quantity, Money UnitCost)>(fifoLayers);

        while (remainingQty > 0 && tempLayers.Count > 0)
        {
            var layer = tempLayers.Dequeue();
            var qtyFromLayer = Math.Min(remainingQty, layer.Quantity.Value);
            
            if (firstLayer)
            {
                totalCost = Money.Zero(layer.UnitCost.Currency);
                firstLayer = false;
            }
            
            totalCost += layer.UnitCost * qtyFromLayer;
            remainingQty -= qtyFromLayer;

            if (layer.Quantity.Value > qtyFromLayer)
            {
                tempLayers.Enqueue((new Quantity(layer.Quantity.Value - qtyFromLayer, layer.Quantity.Unit), layer.UnitCost));
            }
        }

        if (remainingQty > 0)
            throw new InvalidOperationException("Insufficient FIFO layers to cover issue quantity.");

        return totalCost / quantityToIssue.Value;
    }

    public static Money CalculateLifoCost(Stack<(Quantity Quantity, Money UnitCost)> lifoLayers, Quantity quantityToIssue)
    {
        if (quantityToIssue.Value <= 0)
            throw new ArgumentException("Quantity to issue must be positive.", nameof(quantityToIssue));

        var remainingQty = quantityToIssue.Value;
        var totalCost = Money.Zero(Currency.IRR);
        var firstLayer = true;

        var tempLayers = new Stack<(Quantity Quantity, Money UnitCost)>(lifoLayers);

        while (remainingQty > 0 && tempLayers.Count > 0)
        {
            var layer = tempLayers.Pop();
            var qtyFromLayer = Math.Min(remainingQty, layer.Quantity.Value);
            
            if (firstLayer)
            {
                totalCost = Money.Zero(layer.UnitCost.Currency);
                firstLayer = false;
            }
            
            totalCost += layer.UnitCost * qtyFromLayer;
            remainingQty -= qtyFromLayer;

            if (layer.Quantity.Value > qtyFromLayer)
            {
                tempLayers.Push((new Quantity(layer.Quantity.Value - qtyFromLayer, layer.Quantity.Unit), layer.UnitCost));
            }
        }

        if (remainingQty > 0)
            throw new InvalidOperationException("Insufficient LIFO layers to cover issue quantity.");

        return totalCost / quantityToIssue.Value;
    }

    public static Money CalculateStandardCost(Money standardCost) => standardCost;

    public static Money CalculateSpecificCost(Money specificCost) => specificCost;
}