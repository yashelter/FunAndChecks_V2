using Frontend.Shared.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Frontend.Admin.Dialogs;

public partial class CommentDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Inject] private IStringLocalizer<AppStrings> Loc { get; set; } = null!;

    private string _comment = string.Empty;

    private void Submit() => MudDialog.Close(DialogResult.Ok(_comment));

    private void Cancel() => MudDialog.Cancel();
}
