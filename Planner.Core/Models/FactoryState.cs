namespace Planner.Core.Models
{
    public class FactoryState
    {
        // Keeps track of free byproducts currently floating around the factory
        public Dictionary<string, decimal> AvailableByproducts { get; } = new();

        public void AddByproduct(string itemClassName, decimal amount)
        {
            if (!AvailableByproducts.ContainsKey(itemClassName))
                AvailableByproducts[itemClassName] = 0;

            AvailableByproducts[itemClassName] += amount;
        }

        public decimal ConsumeByproduct(string itemClassName, decimal amountNeeded)
        {
            if (!AvailableByproducts.ContainsKey(itemClassName) || AvailableByproducts[itemClassName] <= 0)
                return 0; // We have none to give

            decimal amountToTake = Math.Min(AvailableByproducts[itemClassName], amountNeeded);
            AvailableByproducts[itemClassName] -= amountToTake;

            return amountToTake; // Return how much we successfully scavenged
        }
    }
}