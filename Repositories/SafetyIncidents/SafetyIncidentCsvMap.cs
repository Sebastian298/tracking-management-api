using System.Globalization;
using CsvHelper.Configuration;
using tracking_management_api.Models;

namespace tracking_management_api.Repositories.SafetyIncidents;

internal sealed class SafetyIncidentCsvMap : ClassMap<SafetyIncident>
{
    public SafetyIncidentCsvMap()
    {
        const string dateFormat = "yyyy-MM-dd HH:mm:ss";

        Map(incident => incident.IncidentId).Name("incident_id");
        Map(incident => incident.TripId).Name("trip_id");
        Map(incident => incident.TruckId).Name("truck_id");
        Map(incident => incident.DriverId).Name("driver_id");
        Map(incident => incident.IncidentDate)
            .Name("incident_date")
            .TypeConverterOption.Format(dateFormat)
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
        Map(incident => incident.IncidentType).Name("incident_type");
        Map(incident => incident.LocationCity).Name("location_city");
        Map(incident => incident.LocationState).Name("location_state");
        Map(incident => incident.AtFaultFlag).Name("at_fault_flag");
        Map(incident => incident.InjuryFlag).Name("injury_flag");
        Map(incident => incident.VehicleDamageCost).Name("vehicle_damage_cost");
        Map(incident => incident.CargoDamageCost).Name("cargo_damage_cost");
        Map(incident => incident.ClaimAmount).Name("claim_amount");
        Map(incident => incident.PreventableFlag).Name("preventable_flag");
        Map(incident => incident.Description).Name("description");
    }
}
