using tracking_management_api.Models;

namespace tracking_management_api.Tests;

internal static class LoadFixtures
{
    public static Load CreateLoad(
        string loadId = "LOAD00000001",
        string customerId = "CUST00183",
        string routeId = "RTE00019")
    {
        return new Load(
            loadId,
            customerId,
            routeId,
            new DateOnly(2022, 1, 1),
            "Dry Van",
            19178,
            13,
            3045.23m,
            406.72m,
            100m,
            "Completed",
            "Spot");
    }
}
