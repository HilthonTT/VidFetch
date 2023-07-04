using Microsoft.AspNetCore.Components;

namespace VidFetch.Components;

public partial class LoadingDialog
{
    [Parameter]
    [EditorRequired]
    public bool IsVisible { get; set; }
}