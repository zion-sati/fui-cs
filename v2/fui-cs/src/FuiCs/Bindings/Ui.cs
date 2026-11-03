namespace FuiCs.Bindings;

/// <summary>
/// Thin wrappers over the generated ABI that node and control code calls. Strings and arrays cross the ABI
/// as values, so no UTF-8 encoding happens here.
/// </summary>
internal static class Ui
{
    public static void Reset() => Runtime.Abi.UiReset();
    public static ulong CreateNode(NodeType type) => Runtime.Abi.UiCreateNode(type);
    public static void SetNodeId(ulong handle, string id) => Runtime.Abi.UiSetNodeId(handle, id);
    public static void SetSemanticRole(ulong handle, SemanticRole role) => Runtime.Abi.UiSetSemanticRole(handle, role);
    public static void SetSemanticLabel(ulong handle, string label) => Runtime.Abi.UiSetSemanticLabel(handle, label);
    public static void SetSemanticChecked(ulong handle, SemanticCheckedState checkedState) => Runtime.Abi.UiSetSemanticChecked(handle, checkedState);
    public static void SetSemanticSelected(ulong handle, bool hasSelected, bool selected) => Runtime.Abi.UiSetSemanticSelected(handle, hasSelected, selected);
    public static void SetSemanticExpanded(ulong handle, bool hasExpanded, bool expanded) => Runtime.Abi.UiSetSemanticExpanded(handle, hasExpanded, expanded);
    public static void SetSemanticDisabled(ulong handle, bool hasDisabled, bool disabled) => Runtime.Abi.UiSetSemanticDisabled(handle, hasDisabled, disabled);
    public static void SetSemanticValueRange(ulong handle, bool hasValueRange, float valueNow, float valueMin, float valueMax) => Runtime.Abi.UiSetSemanticValueRange(handle, hasValueRange, valueNow, valueMin, valueMax);
    public static void SetSemanticOrientation(ulong handle, Orientation orientation) => Runtime.Abi.UiSetSemanticOrientation(handle, orientation);
    public static void RequestSemanticAnnouncement(ulong handle) => Runtime.Abi.UiRequestSemanticAnnouncement(handle);
    public static uint PushSemanticScope(ulong handle) => Runtime.Abi.UiPushSemanticScope(handle);
    public static void RemoveSemanticScope(uint token) => Runtime.Abi.UiRemoveSemanticScope(token);
    public static void DeleteNode(ulong handle) => Runtime.Abi.UiDeleteNode(handle);
    public static void AddChild(ulong parent, ulong child) => Runtime.Abi.UiNodeAddChild(parent, child);
    public static void RemoveChild(ulong parent, ulong child) => Runtime.Abi.UiNodeRemoveChild(parent, child);
    public static void SetIsPortal(ulong handle, bool flag) => Runtime.Abi.UiSetIsPortal(handle, flag);
    public static void SetVisibility(ulong handle, Visibility visibility) => Runtime.Abi.UiSetVisibility(handle, visibility);
    public static void SetRoot(ulong handle) => Runtime.Abi.UiSetRoot(handle);
    public static void SetWidth(ulong handle, float value, Unit unit) => Runtime.Abi.UiSetWidth(handle, value, unit);
    public static void SetHeight(ulong handle, float value, Unit unit) => Runtime.Abi.UiSetHeight(handle, value, unit);
    public static void SetFillWidth(ulong handle, bool fill) => Runtime.Abi.UiSetFillWidth(handle, fill);
    public static void SetFillHeight(ulong handle, bool fill) => Runtime.Abi.UiSetFillHeight(handle, fill);
    public static void SetFillWidthPercent(ulong handle, float percent) => Runtime.Abi.UiSetFillWidthPercent(handle, percent);
    public static void SetFillHeightPercent(ulong handle, float percent) => Runtime.Abi.UiSetFillHeightPercent(handle, percent);
    public static void SetMinWidth(ulong handle, float value, Unit unit) => Runtime.Abi.UiSetMinWidth(handle, value, unit);
    public static void SetMaxWidth(ulong handle, float value, Unit unit) => Runtime.Abi.UiSetMaxWidth(handle, value, unit);
    public static void SetMinHeight(ulong handle, float value, Unit unit) => Runtime.Abi.UiSetMinHeight(handle, value, unit);
    public static void SetMaxHeight(ulong handle, float value, Unit unit) => Runtime.Abi.UiSetMaxHeight(handle, value, unit);
    public static void SetFlexDirection(ulong handle, FlexDirection direction) => Runtime.Abi.UiSetFlexDirection(handle, direction);
    public static void SetFlexBasis(ulong handle, float basis) => Runtime.Abi.UiSetFlexBasis(handle, basis);
    public static void SetJustifyContent(ulong handle, JustifyContent justify) => Runtime.Abi.UiSetJustifyContent(handle, justify);
    public static void SetAlignItems(ulong handle, AlignItems align) => Runtime.Abi.UiSetAlignItems(handle, align);
    public static void SetAlignSelf(ulong handle, AlignSelf align) => Runtime.Abi.UiSetAlignSelf(handle, align);
    public static void SetPadding(ulong handle, float left, float top, float right, float bottom) => Runtime.Abi.UiSetPadding(handle, left, top, right, bottom);
    public static void SetMargin(ulong handle, float left, float top, float right, float bottom) => Runtime.Abi.UiSetMargin(handle, left, top, right, bottom);
    public static void SetPositionType(ulong handle, PositionType positionType) => Runtime.Abi.UiSetPositionType(handle, positionType);
    public static void SetPosition(ulong handle, float left, float top, float right, float bottom) => Runtime.Abi.UiSetPosition(handle, left, top, right, bottom);
    public static void SetIsSharedSizeScope(ulong handle, bool isScope) => Runtime.Abi.UiSetIsSharedSizeScope(handle, isScope);
    public static void SetCustomDrawable(ulong handle, bool flag) => Runtime.Abi.UiSetCustomDrawable(handle, flag);
    public static void SetFlexWrap(ulong handle, FlexWrap wrap) => Runtime.Abi.UiSetFlexWrap(handle, wrap);
    public static uint PrepareNode(ulong handle) => Runtime.Abi.UiPrepareNode(handle);
    public static void SetDynamicTextCharset(ulong handle, string charset) => Runtime.Abi.UiSetDynamicTextCharset(handle, charset);
    public static void GridSetColumnSharedSizeGroup(ulong handle, uint index, string group) => Runtime.Abi.UiGridSetColumnSharedSizeGroup(handle, index, group);
    public static void GridSetRowSharedSizeGroup(ulong handle, uint index, string group) => Runtime.Abi.UiGridSetRowSharedSizeGroup(handle, index, group);
    public static void SetGridPlacement(ulong handle, uint row, uint col, uint rowSpan, uint colSpan) => Runtime.Abi.UiNodeSetGridPlacement(handle, row, col, rowSpan, colSpan);
    public static void SetBackgroundColor(ulong handle, uint color) => Runtime.Abi.UiSetBgColor(handle, color);
    public static void SetBoxStyle(ulong handle, uint bgColor, float topLeftRadius, float topRightRadius, float bottomRightRadius, float bottomLeftRadius, float borderWidth, uint borderColor, BorderStyle borderStyle, float borderDashOn, float borderDashOff) => Runtime.Abi.UiSetBoxStyle(handle, bgColor, topLeftRadius, topRightRadius, bottomRightRadius, bottomLeftRadius, borderWidth, borderColor, borderStyle, borderDashOn, borderDashOff);
    public static void SetLayerEffect(ulong handle, float opacity, float blurSigma, uint blendMode) => Runtime.Abi.UiSetLayerEffect(handle, opacity, blurSigma, blendMode);
    public static void SetDropShadow(ulong handle, uint color, float offsetX, float offsetY, float blurSigma, float spread) => Runtime.Abi.UiSetDropShadow(handle, color, offsetX, offsetY, blurSigma, spread);
    public static void SetBackgroundBlur(ulong handle, float blurSigma) => Runtime.Abi.UiSetBackgroundBlur(handle, blurSigma);
    public static void SetImage(ulong handle, uint textureId, ObjectFit objectFit, ImageSamplingKind samplingKind, uint maxAniso) => Runtime.Abi.UiSetImage(handle, textureId, objectFit, samplingKind, maxAniso);
    public static void SetImageNine(ulong handle, uint textureId, float insetLeft, float insetTop, float insetRight, float insetBottom, ImageSamplingKind samplingKind, uint maxAniso) => Runtime.Abi.UiSetImageNine(handle, textureId, insetLeft, insetTop, insetRight, insetBottom, samplingKind, maxAniso);
    public static void SetSvg(ulong handle, uint svgId, uint tintColor, ImageSamplingKind samplingKind, uint maxAniso) => Runtime.Abi.UiSetSvg(handle, svgId, tintColor, samplingKind, maxAniso);
    public static void SetClipToBounds(ulong handle, bool clip) => Runtime.Abi.UiSetClipToBounds(handle, clip);
    public static void SetInteractive(ulong handle, bool flag) => Runtime.Abi.UiSetInteractive(handle, flag);
    public static void SetPreserveSelectionOnPointerDown(ulong handle, bool preserve) => Runtime.Abi.UiSetPreserveSelectionOnPointerDown(handle, preserve);
    public static void SetScrollProxyTarget(ulong handle, ulong scrollHandle) => Runtime.Abi.UiSetScrollProxyTarget(handle, scrollHandle);
    public static void SetScrollEnabled(ulong handle, bool enabledX, bool enabledY) => Runtime.Abi.UiSetScrollEnabled(handle, enabledX, enabledY);
    public static void SetScrollFriction(ulong handle, float friction) => Runtime.Abi.UiSetScrollFriction(handle, friction);
    public static void SetSmoothScrolling(ulong handle, bool smoothScrolling) => Runtime.Abi.UiSetSmoothScrolling(handle, smoothScrolling);
    public static void SetScrollWheelLatching(ulong handle, bool latching) => Runtime.Abi.UiSetScrollWheelLatching(handle, latching);
    public static void SetFocusable(ulong handle, bool flag, int tabIndex) => Runtime.Abi.UiSetFocusable(handle, flag, tabIndex);
    public static void RequestFocus(ulong handle) => Runtime.Abi.UiRequestFocus(handle);
    public static void SetText(ulong handle, string text) => Runtime.Abi.UiSetText(handle, text);
    public static void LoadSvg(uint svgId, string url) => Runtime.Abi.HostLoadSvg(svgId, url);
    public static void LoadTexture(uint textureId, string url) => Runtime.Abi.HostLoadTexture(textureId, url);
    public static void ReleaseSvg(uint svgId) => Runtime.Abi.HostReleaseSvg(svgId);
    public static void ReleaseTexture(uint textureId) => Runtime.Abi.HostReleaseTexture(textureId);
    public static void BitmapCommit(uint textureId, byte[] bytes, uint width, uint height) => Runtime.Abi.HostBitmapCommit(textureId, bytes, width, height);
    public static void BitmapRelease(uint textureId) => Runtime.Abi.HostBitmapRelease(textureId);
    public static void LoadFont(uint fontId, string url) => Runtime.Abi.HostLoadFont(fontId, url);
    public static void StartTimer(uint timerId, int delayMs) => Runtime.Abi.HostStartTimer(timerId, delayMs);
    public static void CancelTimer(uint timerId) => Runtime.Abi.HostCancelTimer(timerId);
    public static void SetFont(ulong handle, uint fontId, float size) => Runtime.Abi.UiSetFont(handle, fontId, size);
    public static void SetLineHeight(ulong handle, float lineHeight) => Runtime.Abi.UiSetLineHeight(handle, lineHeight);
    public static void RegisterFontFallback(uint fontId, uint fallbackFontId) => Runtime.Abi.UiRegisterFontFallback(fontId, fallbackFontId);
    public static void SetTextColor(ulong handle, uint color) => Runtime.Abi.UiSetTextColor(handle, color);
    public static void SetTextAlign(ulong handle, TextAlign align) => Runtime.Abi.UiSetTextAlign(handle, align);
    public static void SetTextVerticalAlign(ulong handle, TextVerticalAlign align) => Runtime.Abi.UiSetTextVerticalAlign(handle, align);
    public static void SetTextLimits(ulong handle, int maxChars, int maxLines) => Runtime.Abi.UiSetTextLimits(handle, maxChars, maxLines);
    public static void SetTextWrapping(ulong handle, bool wrap) => Runtime.Abi.UiSetTextWrapping(handle, wrap);
    public static void SetTextOverflow(ulong handle, TextOverflow overflow) => Runtime.Abi.UiSetTextOverflow(handle, overflow);
    public static void SetTextOverflowFade(ulong handle, bool horizontal, bool vertical) => Runtime.Abi.UiSetTextOverflowFade(handle, horizontal, vertical);
    public static void SetTextObscured(ulong handle, bool obscured) => Runtime.Abi.UiSetTextObscured(handle, obscured);
    public static void SetEditable(ulong handle, bool editable) => Runtime.Abi.UiSetEditable(handle, editable);
    public static void SetEditorCommandKeys(ulong handle, bool enabled) => Runtime.Abi.UiSetEditorCommandKeys(handle, enabled);
    public static void SetEditorAcceptsTab(ulong handle, bool enabled) => Runtime.Abi.UiSetEditorAcceptsTab(handle, enabled);
    public static void SetCaretColor(ulong handle, uint color) => Runtime.Abi.UiSetCaretColor(handle, color);
    public static void SetScrollOffset(ulong handle, float offsetX, float offsetY) => Runtime.Abi.UiSetScrollOffset(handle, offsetX, offsetY);
    public static void SetScrollContentSize(ulong handle, float contentWidth, float contentHeight) => Runtime.Abi.UiSetScrollContentSize(handle, contentWidth, contentHeight);
    public static void ClearMomentumScroll() => Runtime.Abi.UiClearMomentumScroll();
    public static void SetSelectable(ulong handle, bool selectable, uint selectionColor) => Runtime.Abi.UiSetSelectable(handle, selectable, selectionColor);
    public static void SetSelectionArea(ulong handle, bool isArea) => Runtime.Abi.UiSetSelectionArea(handle, isArea);
    public static void SetSelectionAreaBarrier(ulong handle, bool isBarrier) => Runtime.Abi.UiSetSelectionAreaBarrier(handle, isBarrier);
    public static void ClearSelection(ulong handle) => Runtime.Abi.UiClearSelection(handle);
    public static void RetargetSelection(ulong fromHandle, ulong toHandle) => Runtime.Abi.UiRetargetSelection(fromHandle, toHandle);
    public static bool IsPointInSelection(float x, float y) => Runtime.Abi.UiIsPointInSelection(x, y);
    public static void SetTextSelectionRange(ulong handle, uint selectionStart, uint selectionEnd) => Runtime.Abi.UiSetTextSelectionRange(handle, selectionStart, selectionEnd);
    public static bool SelectWordAt(ulong handle, float x, float y) => Runtime.Abi.UiSelectWordAt(handle, x, y);
    public static bool BeginSelectionEndpointDrag(ulong handle, uint endpoint) => Runtime.Abi.UiBeginSelectionEndpointDrag(handle, endpoint);
    public static void ClearCurrentSelection() => Runtime.Abi.UiClearCurrentSelection();
    public static void CopyCurrentSelection() => Runtime.Abi.UiCopyCurrentSelection();
    public static bool CanUndoTextEdit(ulong handle) => Runtime.Abi.UiCanUndoTextEdit(handle);
    public static bool CanRedoTextEdit(ulong handle) => Runtime.Abi.UiCanRedoTextEdit(handle);
    public static bool HasTextSelection(ulong handle) => Runtime.Abi.UiHasTextSelection(handle);
    public static void UndoTextEdit(ulong handle) => Runtime.Abi.UiUndoTextEdit(handle);
    public static void RedoTextEdit(ulong handle) => Runtime.Abi.UiRedoTextEdit(handle);
    public static void CopyTextSelection(ulong handle) => Runtime.Abi.UiCopyTextSelection(handle);
    public static void CutTextSelection(ulong handle) => Runtime.Abi.UiCutTextSelection(handle);
    public static void ReplaceTextRange(ulong handle, uint start, uint end, string text, uint caret) => Runtime.Abi.UiReplaceTextRange(handle, start, end, text, caret);
    public static void PasteText(ulong handle) => Runtime.Abi.UiPasteText(handle);
    public static void SelectAllText(ulong handle) => Runtime.Abi.UiSelectAllText(handle);
    public static void CommitFrame() => Runtime.Abi.UiCommitFrame();
    public static void ResizeWindow(float width, float height) => Runtime.Abi.UiResizeWindow(width, height);
    public static void RequestRender() => Runtime.Abi.HostRequestRender();
    public static float GetViewportWidth() => Runtime.Abi.HostGetViewportWidth();
    public static float GetViewportHeight() => Runtime.Abi.HostGetViewportHeight();
    public static float GetDevicePixelRatio() => Runtime.Abi.HostGetDevicePixelRatio();

    public static void RegisterTextInputMetadata(ulong handle, bool isPassword, string? hostAutofillHint) =>
        Runtime.Abi.HostRegisterTextInputMetadata(handle, isPassword, string.IsNullOrEmpty(hostAutofillHint) ? "" : hostAutofillHint);

    /// <summary>The node's text layout metrics; false when the node has no text layout.</summary>
    public static bool TryGetTextMetrics(ulong handle, out TextMetrics metrics)
    {
        var found = Runtime.Abi.UiGetTextMetrics(handle, out var width, out var height, out var baseline, out var lineCount, out var maxLineWidth);
        metrics = found ? new TextMetrics(width, height, baseline, lineCount, maxLineWidth) : default;
        return found;
    }

    public static void GridSetColumns(ulong handle, float[] values, byte[] types) =>
        Runtime.Abi.UiGridSetColumns(handle, (uint)values.Length, values, types);

    public static void GridSetRows(ulong handle, float[] values, byte[] types) =>
        Runtime.Abi.UiGridSetRows(handle, (uint)values.Length, values, types);

    public static void SetLinearGradient(ulong handle, float startX, float startY, float endX, float endY, float[] offsets, uint[] colors)
    {
        if (offsets.Length == 0)
        {
            return;
        }
        Runtime.Abi.UiSetLinearGradient(handle, startX, startY, endX, endY, (uint)offsets.Length, offsets, colors);
    }

    /// <summary>Each style run is seven words.</summary>
    public static void SetTextStyleRuns(ulong handle, uint[] runsWords) =>
        Runtime.Abi.UiSetTextStyleRuns(handle, (uint)(runsWords.Length / 7), runsWords);

    /// <summary>The node's bounds in scene pixels; false when the node has no layout yet.</summary>
    public static bool TryGetBounds(ulong handle, out Rect bounds)
    {
        var found = Runtime.Abi.UiGetBounds(handle, out var x, out var y, out var width, out var height);
        bounds = found ? new Rect(x, y, width, height) : default;
        return found;
    }

    /// <summary>The visible part of the node's bounds; false when the node has no layout yet.</summary>
    public static bool TryGetVisibleBounds(ulong handle, out Rect bounds)
    {
        var found = Runtime.Abi.UiGetVisibleBounds(handle, out var x, out var y, out var width, out var height);
        bounds = found ? new Rect(x, y, width, height) : default;
        return found;
    }
}
