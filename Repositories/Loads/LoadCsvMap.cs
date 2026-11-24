using CsvHelper.Configuration;
using tracking_management_api.Models;

namespace tracking_management_api.Repositories.Loads;

internal sealed class LoadCsvMap : ClassMap<Load>
{
    public LoadCsvMap()
    {
        Parameter("LoadId").Name("load_id");
        Parameter("CustomerId").Name("customer_id");
        Parameter("RouteId").Name("route_id");
        Parameter("LoadDate").Name("load_date");
        Parameter("LoadType").Name("load_type");
        Parameter("WeightLbs").Name("weight_lbs");
        Parameter("Pieces").Name("pieces");
        Parameter("Revenue").Name("revenue");
        Parameter("FuelSurcharge").Name("fuel_surcharge");
        Parameter("AccessorialCharges").Name("accessorial_charges");
        Parameter("LoadStatus").Name("load_status");
        Parameter("BookingType").Name("booking_type");
    }
}
