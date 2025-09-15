namespace Gastos.Pwa.Components;

public class StateContainer
{
    #region PAGE INFO
    public event Action? PageTitleChanged;

    private string _pageTitle = string.Empty;

    public string PageTitle
    {
        get
        {
            return _pageTitle;
        }
        set
        {
            _pageTitle = value;
            PageTitleChanged?.Invoke();
        }
    }
    public bool IsPageHome { get; set; } = true;
    #endregion


    #region ANALYSIS JOBS
    public event Action<List<Guid>>? AnalysisJobCreated;

    public void NotifyAnalysisJobCreated(List<Guid> jobIds)
    {
        AnalysisJobCreated?.Invoke(jobIds); // Pass the correct argument to match the delegate signature
    }
    #endregion


    #region PARAMETERS
    public ProductParameters ProductParams { get; set; } = new();
    public StoreParameters StoreParams { get; set; } = new();
    public ReceiptParameters ReceiptParams { get; set; } = new();
    public StatParameters StatsParams { get; set; } = new();
    #endregion


    #region BREAKPOINTS
    public event Action? BreakpointChanged;

    private Breakpoint _currentBreakpoint = Breakpoint.None;
    public Breakpoint CurrentBreakpoint
    {
        get => _currentBreakpoint;
        set
        {
            bool previousIsMobile = IsMobile;

            _currentBreakpoint = value;

            if (previousIsMobile != IsMobile)
            {
                BreakpointChanged?.Invoke();
            }
        }
    }

    public bool IsMobile => CurrentBreakpoint is Breakpoint.Xs || CurrentBreakpoint is Breakpoint.Sm;
    public bool IsNotMobile => !IsMobile;
    #endregion
}
