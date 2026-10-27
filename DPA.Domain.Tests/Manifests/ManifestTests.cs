using DPA.Domain.Cargo;
using DPA.Domain.Manifests;
using DPA.Domain.Shared;
using DPA.Domain.Vehicles;

namespace DPA.Domain.Tests.Manifests;

public class ManifestTests
{
    // ---------- Shared test data ----------

    private static readonly DateTimeOffset Now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly Guid _plannerId = Guid.NewGuid();
    private readonly Guid _teamLeadId = Guid.NewGuid();

    // Semi-trailer: 13.6 m long, front support 1.0 m from the nose
    // → nose wall at x = -1.0, rear doors at x = 12.6, side walls at y = ±1.24
    private readonly Vehicle _vehicle = new(new VehicleProfile("Semi-trailer", 0.5m, 2.0m),
        13.6m, "TR-01", 2.48m, 2.7m, 7000m, 7.6m, 6.2m, 1.0m, 24000m);

    // ---------- Helpers: build a manifest in a given status ----------

    private static void AssertSuccess(Result result) =>
        Assert.True(result.IsSuccess, result.Error?.Message);

    private static Manifest CreateManifest() => new(Guid.NewGuid(), Now.AddDays(1));

    private static CargoItem CreatePallet(decimal heightM = 1.5m, decimal weightKg = 500m) =>
        new("PAL-001", 1.2m, 0.8m, heightM, weightKg, Stability.Stable);

    private static List<CargoItem> CreatePalletsOnDock(Manifest manifest, int count)
    {
        var pallets = new List<CargoItem>();

        for (var i = 0; i < count; i++)
        {
            var pallet = CreatePallet();
            pallet.MoveToDock(Now);
            pallet.AssignToManifest(manifest.Id);
            pallets.Add(pallet);
        }

        return pallets;
    }

    private (Manifest Manifest, List<CargoItem> Pallets) CreateUnderPlanning(int palletCount = 2)
    {
        var manifest = CreateManifest();
        var pallets = CreatePalletsOnDock(manifest, palletCount);

        AssertSuccess(manifest.MarkReadyToPlan(pallets));
        AssertSuccess(manifest.StartPlanning(_vehicle));

        return (manifest, pallets);
    }

    // Puts the pallets in a row along the trailer, centered across it
    private void PlaceAll(Manifest manifest, List<CargoItem> pallets)
    {
        for (var i = 0; i < pallets.Count; i++)
        {
            AssertSuccess(manifest.PlaceCargo(_vehicle, pallets[i], 0.6m + i * 1.2m, 0m, false));
        }
    }

    private (Manifest Manifest, List<CargoItem> Pallets) CreateAwaitingConfirmation(bool truckArrived = true)
    {
        var (manifest, pallets) = CreateUnderPlanning();
        PlaceAll(manifest, pallets);

        AssertSuccess(manifest.Confirm(pallets, _plannerId, "A. Planner"));
        AssertSuccess(manifest.MarkAtLoadingArea());

        if (truckArrived)
        {
            AssertSuccess(manifest.MarkTruckArrived(Now));
        }

        return (manifest, pallets);
    }

    private Manifest CreateLoading()
    {
        var (manifest, _) = CreateAwaitingConfirmation();
        AssertSuccess(manifest.Approve(_teamLeadId, "S. Lead"));
        return manifest;
    }

    // ---------- Constructor ----------

    [Fact]
    public void Constructor_ValidValues_StartsAwaitingPallets()
    {
        var manifest = CreateManifest();

        Assert.Equal(ManifestStatus.AwaitingPallets, manifest.Status);
        Assert.Empty(manifest.Placements);
    }

    [Fact]
    public void Constructor_EmptyDestinationId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Manifest(Guid.Empty, Now));
    }

    [Fact]
    public void Constructor_DefaultRequiredArrival_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Manifest(Guid.NewGuid(), default));
    }

    // ---------- MarkReadyToPlan ----------

    [Fact]
    public void MarkReadyToPlan_AllPalletsOnDock_MovesToAwaitingPlanning()
    {
        var manifest = CreateManifest();
        var pallets = CreatePalletsOnDock(manifest, 2);

        var result = manifest.MarkReadyToPlan(pallets);

        Assert.True(result.IsSuccess);
        Assert.Equal(ManifestStatus.AwaitingPlanning, manifest.Status);
    }

    [Fact]
    public void MarkReadyToPlan_NoPallets_ReturnsRuleViolation()
    {
        var manifest = CreateManifest();

        var result = manifest.MarkReadyToPlan(new List<CargoItem>());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
    }

    [Fact]
    public void MarkReadyToPlan_PalletStillInStorage_ReturnsRuleViolation()
    {
        var manifest = CreateManifest();
        var pallets = CreatePalletsOnDock(manifest, 1);
        var inStorage = CreatePallet();
        inStorage.AssignToManifest(manifest.Id);
        pallets.Add(inStorage);

        var result = manifest.MarkReadyToPlan(pallets);

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
        Assert.Equal(ManifestStatus.AwaitingPallets, manifest.Status);
    }

    [Fact]
    public void MarkReadyToPlan_CalledTwice_ReturnsConflict()
    {
        var manifest = CreateManifest();
        var pallets = CreatePalletsOnDock(manifest, 1);
        AssertSuccess(manifest.MarkReadyToPlan(pallets));

        var result = manifest.MarkReadyToPlan(pallets);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void MarkReadyToPlan_PalletFromAnotherManifest_ThrowsArgumentException()
    {
        var manifest = CreateManifest();
        var otherManifest = CreateManifest();
        var pallets = CreatePalletsOnDock(otherManifest, 1);

        Assert.Throws<ArgumentException>(() => manifest.MarkReadyToPlan(pallets));
    }

    [Fact]
    public void MarkReadyToPlan_NullList_ThrowsArgumentNullException()
    {
        var manifest = CreateManifest();

        Assert.Throws<ArgumentNullException>(() => manifest.MarkReadyToPlan(null!));
    }

    // ---------- StartPlanning ----------

    [Fact]
    public void StartPlanning_AwaitingPlanning_AssignsVehicleAndMovesToUnderPlanning()
    {
        var (manifest, _) = CreateUnderPlanning();

        Assert.Equal(ManifestStatus.UnderPlanning, manifest.Status);
        Assert.Equal(_vehicle.Id, manifest.VehicleId);
    }

    [Fact]
    public void StartPlanning_WrongStatus_ReturnsConflict()
    {
        var manifest = CreateManifest();

        var result = manifest.StartPlanning(_vehicle);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Null(manifest.VehicleId);
    }

    // ---------- PlaceCargo ----------

    [Fact]
    public void PlaceCargo_ValidPosition_AddsPlacement()
    {
        var (manifest, pallets) = CreateUnderPlanning();

        var result = manifest.PlaceCargo(_vehicle, pallets[0], 2m, 0m, false);

        Assert.True(result.IsSuccess);
        var placement = Assert.Single(manifest.Placements);
        Assert.Equal(pallets[0].Id, placement.CargoItemId);
    }

    [Fact]
    public void PlaceCargo_DifferentVehicle_ThrowsArgumentException()
    {
        var (manifest, pallets) = CreateUnderPlanning();
        var otherVehicle = new Vehicle(new VehicleProfile("Semi-trailer", 0.5m, 2.0m),
            13.6m, "TR-02", 2.48m, 2.7m, 7000m, 7.6m, 6.2m, 1.0m, 24000m);

        Assert.Throws<ArgumentException>(() =>
            manifest.PlaceCargo(otherVehicle, pallets[0], 2m, 0m, false));
    }

    [Fact]
    public void PlaceCargo_NotUnderPlanning_ReturnsConflict()
    {
        var (manifest, pallets) = CreateAwaitingConfirmation();

        var result = manifest.PlaceCargo(_vehicle, pallets[0], 8m, 0m, false);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void PlaceCargo_PalletFromAnotherManifest_ReturnsRuleViolation()
    {
        var (manifest, _) = CreateUnderPlanning();
        var stranger = CreatePallet();
        stranger.AssignToManifest(Guid.NewGuid());

        var result = manifest.PlaceCargo(_vehicle, stranger, 2m, 0m, false);

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
    }

    [Fact]
    public void PlaceCargo_AlreadyPlaced_ReturnsConflict()
    {
        var (manifest, pallets) = CreateUnderPlanning();
        AssertSuccess(manifest.PlaceCargo(_vehicle, pallets[0], 2m, 0m, false));

        var result = manifest.PlaceCargo(_vehicle, pallets[0], 6m, 0m, false);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void PlaceCargo_TallerThanVehicle_ReturnsRuleViolation()
    {
        var (manifest, _) = CreateUnderPlanning();
        var tall = CreatePallet(heightM: 3.0m);
        tall.AssignToManifest(manifest.Id);

        var result = manifest.PlaceCargo(_vehicle, tall, 2m, 0m, false);

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
    }

    [Fact]
    public void PlaceCargo_HeavierThanPayload_ReturnsRuleViolation()
    {
        var (manifest, _) = CreateUnderPlanning();
        var heavy = CreatePallet(weightKg: 25000m);
        heavy.AssignToManifest(manifest.Id);

        var result = manifest.PlaceCargo(_vehicle, heavy, 2m, 0m, false);

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
    }

    [Theory]
    [InlineData(-0.6, 0.0, false)]  // front edge -1.2 is past the nose wall at -1.0
    [InlineData(12.3, 0.0, false)]  // rear edge 12.9 is past the rear doors at 12.6
    [InlineData(5.0, 1.0, false)]   // right edge 1.4 is past the wall at 1.24
    [InlineData(5.0, -1.0, false)]  // left edge -1.4 is past the wall at -1.24
    [InlineData(5.0, 0.7, true)]    // rotated: 1.2 m across, right edge 1.3 is past 1.24
    public void PlaceCargo_OutsideVehicle_ReturnsRuleViolation(double xm, double ym, bool isRotated)
    {
        var (manifest, pallets) = CreateUnderPlanning();

        var result = manifest.PlaceCargo(_vehicle, pallets[0], (decimal)xm, (decimal)ym, isRotated);

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
        Assert.Empty(manifest.Placements);
    }

    [Fact]
    public void PlaceCargo_SamePositionNotRotated_Fits()
    {
        var (manifest, pallets) = CreateUnderPlanning();

        // Not rotated: 0.8 m across, right edge 1.1 is inside 1.24
        var result = manifest.PlaceCargo(_vehicle, pallets[0], 5m, 0.7m, false);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlaceCargo_OverlappingAnotherPallet_ReturnsConflict()
    {
        var (manifest, pallets) = CreateUnderPlanning();
        AssertSuccess(manifest.PlaceCargo(_vehicle, pallets[0], 2m, 0m, false));

        var result = manifest.PlaceCargo(_vehicle, pallets[1], 2.5m, 0m, false);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void PlaceCargo_TouchingAnotherPallet_IsAllowed()
    {
        var (manifest, pallets) = CreateUnderPlanning();
        AssertSuccess(manifest.PlaceCargo(_vehicle, pallets[0], 2m, 0m, false));   // x 1.4 → 2.6

        var result = manifest.PlaceCargo(_vehicle, pallets[1], 3.2m, 0m, false);   // x 2.6 → 3.8

        Assert.True(result.IsSuccess);
        Assert.Equal(2, manifest.Placements.Count);
    }

    // ---------- Confirm ----------

    [Fact]
    public void Confirm_AllPalletsPlaced_MovesToSentToLoadingAndSavesPlanner()
    {
        var (manifest, pallets) = CreateUnderPlanning();
        PlaceAll(manifest, pallets);

        var result = manifest.Confirm(pallets, _plannerId, "A. Planner");

        Assert.True(result.IsSuccess);
        Assert.Equal(ManifestStatus.SentToLoading, manifest.Status);
        Assert.Equal(_plannerId, manifest.ConfirmedByUserId);
        Assert.Equal("A. Planner", manifest.ConfirmedByName);
    }

    [Fact]
    public void Confirm_UnplacedPallet_ReturnsRuleViolation()
    {
        var (manifest, pallets) = CreateUnderPlanning();
        AssertSuccess(manifest.PlaceCargo(_vehicle, pallets[0], 2m, 0m, false));

        var result = manifest.Confirm(pallets, _plannerId, "A. Planner");

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
        Assert.Equal(ManifestStatus.UnderPlanning, manifest.Status);
    }

    [Fact]
    public void Confirm_NoPallets_ReturnsRuleViolation()
    {
        var (manifest, _) = CreateUnderPlanning();

        var result = manifest.Confirm(new List<CargoItem>(), _plannerId, "A. Planner");

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
    }

    [Fact]
    public void Confirm_WrongStatus_ReturnsConflictBeforeCheckingPallets()
    {
        var manifest = CreateManifest();

        var result = manifest.Confirm(new List<CargoItem>(), _plannerId, "A. Planner");

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void Confirm_EmptyPlannerId_ThrowsArgumentException()
    {
        var (manifest, pallets) = CreateUnderPlanning();

        Assert.Throws<ArgumentException>(() => manifest.Confirm(pallets, Guid.Empty, "A. Planner"));
    }

    // ---------- MarkAtLoadingArea ----------

    [Fact]
    public void MarkAtLoadingArea_SentToLoading_MovesToAwaitingConfirmation()
    {
        var (manifest, pallets) = CreateUnderPlanning();
        PlaceAll(manifest, pallets);
        AssertSuccess(manifest.Confirm(pallets, _plannerId, "A. Planner"));

        var result = manifest.MarkAtLoadingArea();

        Assert.True(result.IsSuccess);
        Assert.Equal(ManifestStatus.AwaitingConfirmation, manifest.Status);
    }

    [Fact]
    public void MarkAtLoadingArea_WrongStatus_ReturnsConflict()
    {
        var manifest = CreateManifest();

        var result = manifest.MarkAtLoadingArea();

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    // ---------- MarkTruckArrived ----------

    [Fact]
    public void MarkTruckArrived_VehicleAssigned_SetsArrivalTime()
    {
        var (manifest, _) = CreateUnderPlanning();

        var result = manifest.MarkTruckArrived(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, manifest.TruckArrivedAt);
    }

    [Fact]
    public void MarkTruckArrived_NoVehicle_ReturnsRuleViolation()
    {
        var manifest = CreateManifest();

        var result = manifest.MarkTruckArrived(Now);

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
        Assert.Null(manifest.TruckArrivedAt);
    }

    [Fact]
    public void MarkTruckArrived_CalledTwice_ReturnsConflict()
    {
        var (manifest, _) = CreateUnderPlanning();
        AssertSuccess(manifest.MarkTruckArrived(Now));

        var result = manifest.MarkTruckArrived(Now.AddMinutes(10));

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(Now, manifest.TruckArrivedAt);
    }

    // ---------- Approve ----------

    [Fact]
    public void Approve_ValidTeamLead_MovesToLoadingAndSavesApprover()
    {
        var (manifest, _) = CreateAwaitingConfirmation();

        var result = manifest.Approve(_teamLeadId, "S. Lead");

        Assert.True(result.IsSuccess);
        Assert.Equal(ManifestStatus.Loading, manifest.Status);
        Assert.Equal(_teamLeadId, manifest.ApprovedByUserId);
    }

    [Fact]
    public void Approve_SamePersonWhoConfirmed_ReturnsRuleViolation()
    {
        var (manifest, _) = CreateAwaitingConfirmation();

        var result = manifest.Approve(_plannerId, "A. Planner");

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
        Assert.Equal(ManifestStatus.AwaitingConfirmation, manifest.Status);
    }

    [Fact]
    public void Approve_TruckNotArrived_ReturnsRuleViolation()
    {
        var (manifest, _) = CreateAwaitingConfirmation(truckArrived: false);

        var result = manifest.Approve(_teamLeadId, "S. Lead");

        Assert.Equal(ErrorType.RuleViolation, result.Error!.Type);
    }

    [Fact]
    public void Approve_WrongStatus_ReturnsConflict()
    {
        var (manifest, _) = CreateUnderPlanning();

        var result = manifest.Approve(_teamLeadId, "S. Lead");

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void Approve_BlankName_ThrowsArgumentException()
    {
        var (manifest, _) = CreateAwaitingConfirmation();

        Assert.Throws<ArgumentException>(() => manifest.Approve(_teamLeadId, "   "));
    }

    // ---------- Reject ----------

    [Fact]
    public void Reject_WithComment_BackToUnderPlanningAndKeepsComment()
    {
        var (manifest, _) = CreateAwaitingConfirmation();

        var result = manifest.Reject(_teamLeadId, "S. Lead", "Too heavy in the back", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ManifestStatus.UnderPlanning, manifest.Status);
        Assert.Null(manifest.ConfirmedByUserId);
        var rejection = Assert.Single(manifest.Rejections);
        Assert.Equal("Too heavy in the back", rejection.RejectionMessage);
        Assert.Equal(Now, rejection.RejectionDate);
    }

    [Fact]
    public void Reject_TwiceAfterReconfirming_KeepsBothComments()
    {
        var (manifest, pallets) = CreateAwaitingConfirmation();
        AssertSuccess(manifest.Reject(_teamLeadId, "S. Lead", "First comment", Now));
        AssertSuccess(manifest.Confirm(pallets, _plannerId, "A. Planner"));
        AssertSuccess(manifest.MarkAtLoadingArea());

        var result = manifest.Reject(_teamLeadId, "S. Lead", "Second comment", Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, manifest.Rejections.Count);
    }

    [Fact]
    public void Reject_BlankComment_ThrowsArgumentException()
    {
        var (manifest, _) = CreateAwaitingConfirmation();

        Assert.Throws<ArgumentException>(() => manifest.Reject(_teamLeadId, "S. Lead", "   ", Now));
    }

    [Fact]
    public void Reject_WrongStatus_ReturnsConflict()
    {
        var (manifest, _) = CreateUnderPlanning();

        var result = manifest.Reject(_teamLeadId, "S. Lead", "Not ready", Now);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Empty(manifest.Rejections);
    }

    // ---------- Dispatch ----------

    [Fact]
    public void Dispatch_Loading_MovesToDispatchedAndSetsTime()
    {
        var manifest = CreateLoading();

        var result = manifest.Dispatch(Now.AddHours(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(ManifestStatus.Dispatched, manifest.Status);
        Assert.Equal(Now.AddHours(2), manifest.DispatchedAt);
    }

    [Fact]
    public void Dispatch_WrongStatus_ReturnsConflict()
    {
        var (manifest, _) = CreateAwaitingConfirmation();

        var result = manifest.Dispatch(Now);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Null(manifest.DispatchedAt);
    }
}