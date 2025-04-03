using StardewValley;

namespace AnotherTaxMod.Integrations;

public interface IDaLionWalkOfLifeApi
{

    float GetConservationistTaxDeduction(Farmer? farmer = null);

}
