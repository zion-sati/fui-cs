# FUI-CS API reference

One page per public type, generated from the SDK's XML documentation by `npm run generate:docs`.

## Controls

| Type | Kind | Summary |
|---|---|---|
| [`AntiSelectionArea`](AntiSelectionArea.md) | class | Keeps an enclosing [`SelectionArea`](SelectionArea.md) from selecting the text inside it. |
| [`Button`](Button.md) | class | A push button. |
| [`ButtonColors`](ButtonColors.md) | record | Colors a [`Button`](Button.md) uses instead of the theme's; unset colors follow the theme. |
| [`ButtonExtensions`](ButtonExtensions.md) | class | Fluent configuration for [`Button`](Button.md). |
| [`ButtonPresenter`](ButtonPresenter.md) | class | The content a [`ButtonTemplate`](ButtonTemplate.md) builds inside a button: a root and the label text. |
| [`ButtonTemplate`](ButtonTemplate.md) | class | Creates the content of a [`Button`](Button.md), as WPF's control templates do. |
| [`ButtonVisualState`](ButtonVisualState.md) | record struct | The state a button's presenter styles. |
| [`Checkbox`](Checkbox.md) | class | A checkbox. |
| [`CheckboxExtensions`](CheckboxExtensions.md) | class | Fluent configuration for [`Checkbox`](Checkbox.md). |
| [`ComboBox`](ComboBox.md) | class | A text field with a list of [`ComboBox.Items`](ComboBox.md) that filters as the user types. |
| [`ComboBoxCommitMode`](ComboBoxCommitMode.md) | enum | What a combo box does with its text when the list closes or the field loses focus. |
| [`ComboBoxExtensions`](ComboBoxExtensions.md) | class | Fluent configuration for [`ComboBox`](ComboBox.md). |
| [`ComboBoxFilterMode`](ComboBoxFilterMode.md) | enum | Which items a combo box lists for its text. |
| [`ComboBoxItem`](ComboBoxItem.md) | record | An item of a [`ComboBox`](ComboBox.md). |
| [`ContextMenu`](ContextMenu.md) | class | A menu of [`ContextMenu.Items`](ContextMenu.md) shown at a point over the page. |
| [`ContextMenuAction`](ContextMenuAction.md) | enum | A built-in action a menu item runs through the host. |
| [`ContextMenuExtensions`](ContextMenuExtensions.md) | class | Fluent configuration for [`ContextMenu`](ContextMenu.md) and nodes' context menus. |
| [`Dialog`](Dialog.md) | class | A modal dialog: a card with a title, body text, optional content and OK and Cancel buttons, centered over a blurred backdrop that covers the viewport wherever the dialog is mounted. |
| [`DialogExtensions`](DialogExtensions.md) | class | Fluent configuration for [`Dialog`](Dialog.md) and [`Form`](Form.md). |
| [`Dropdown`](Dropdown.md) | class | Picks one of [`Dropdown.Items`](Dropdown.md) from a list that opens under its field. |
| [`DropdownChevronPresenter`](DropdownChevronPresenter.md) | class | The chevron a [`DropdownChevronTemplate`](DropdownChevronTemplate.md) builds; [`DropdownChevronPresenter.Apply`](DropdownChevronPresenter.md) restyles it for each state. |
| [`DropdownChevronTemplate`](DropdownChevronTemplate.md) | class | Creates a dropdown's chevron for its sizing. |
| [`DropdownChevronVisualState`](DropdownChevronVisualState.md) | record struct | The state a dropdown's chevron shows. |
| [`DropdownColors`](DropdownColors.md) | record | Colors a dropdown uses instead of the theme's; unset colors follow the theme. |
| [`DropdownExtensions`](DropdownExtensions.md) | class | Fluent configuration for [`Dropdown`](Dropdown.md). |
| [`DropdownFieldMetrics`](DropdownFieldMetrics.md) | record struct | The field's size and padding, which the dropdown also uses to size its list. |
| [`DropdownFieldPresenter`](DropdownFieldPresenter.md) | class | The field a [`DropdownFieldTemplate`](DropdownFieldTemplate.md) builds for a dropdown: a root holding the host of the selected value's text and the host the chevron goes in. |
| [`DropdownFieldTemplate`](DropdownFieldTemplate.md) | class | Creates a dropdown's field for its sizing, as WPF's control templates do. |
| [`DropdownFieldVisualState`](DropdownFieldVisualState.md) | record struct | The state a dropdown field shows, with the label of the selected option. |
| [`DropdownItem`](DropdownItem.md) | record | An option of a [`Dropdown`](Dropdown.md): its value and the label the field and list show. |
| [`DropdownOptionRowMetrics`](DropdownOptionRowMetrics.md) | record struct | An option row's height, horizontal padding and font size. |
| [`DropdownOptionRowPresenter`](DropdownOptionRowPresenter.md) | class | One row of a dropdown's list, which a [`DropdownOptionRowTemplate`](DropdownOptionRowTemplate.md) builds: a root holding the option's label. |
| [`DropdownOptionRowTemplate`](DropdownOptionRowTemplate.md) | class | Creates the rows of a dropdown's list for its sizing. |
| [`DropdownOptionRowVisualState`](DropdownOptionRowVisualState.md) | record struct | The state a dropdown's option row shows. |
| [`DropdownSizing`](DropdownSizing.md) | record | Sizes for a dropdown's field, option rows and chevron; unset sizes follow the default templates. |
| [`Form`](Form.md) | class | A column grouping a form's fields. |
| [`IndicatorPresenter`](IndicatorPresenter.md) | class | The indicator a [`IndicatorTemplate`](IndicatorTemplate.md) builds for a checkbox, radio button or switch: a root node that [`IndicatorPresenter.Apply`](IndicatorPresenter.md) restyles for each state. |
| [`IndicatorTemplate`](IndicatorTemplate.md) | class | Creates the indicator of a checkbox, radio button or switch for its sizing, as WPF's control templates do. |
| [`IndicatorVisualState`](IndicatorVisualState.md) | record struct | The state an indicator shows. |
| [`LabeledControlColors`](LabeledControlColors.md) | record | Colors a checkbox, radio button or switch uses instead of the theme's; unset colors follow the theme. |
| [`LabeledControlSizing`](LabeledControlSizing.md) | record | Sizes for a checkbox, radio button or switch; unset sizes follow the default template and the theme. |
| [`MenuItem`](MenuItem.md) | class | An entry of a [`ContextMenu`](ContextMenu.md): a label, an optional shortcut hint, and the action it runs when chosen. |
| [`NavigateEventArgs`](NavigateEventArgs.md) | class | A link navigating. |
| [`NavLink`](NavLink.md) | class | A link around any content. |
| [`NavLinkExtensions`](NavLinkExtensions.md) | class | Fluent configuration for [`NavLink`](NavLink.md). |
| [`NavLinkInteractionState`](NavLinkInteractionState.md) | record struct | What a link's content can restyle for: hover, a press in progress, focus and enabled state. |
| [`Popup`](Popup.md) | class | A popup surface shown over the page at a point or next to an anchor, over an optional backdrop. |
| [`PopupExtensions`](PopupExtensions.md) | class | Fluent configuration for [`Popup`](Popup.md) and its subclasses. |
| [`PopupPlacement`](PopupPlacement.md) | enum | Where an anchored popup opens relative to its anchor. |
| [`PressableLabeledControl`](PressableLabeledControl.md) | class | The base of checkboxes, radio buttons and switches: an indicator and a label in a row. |
| [`PressableLabeledControlExtensions`](PressableLabeledControlExtensions.md) | class | Fluent configuration for checkboxes, radio buttons and switches. |
| [`ProgressBar`](ProgressBar.md) | class | A bar that fills from its start in proportion to [`ProgressBar.Value`](ProgressBar.md) between [`ProgressBar.Minimum`](ProgressBar.md) and [`ProgressBar.Maximum`](ProgressBar.md); a vertical bar fills from the bottom. |
| [`ProgressBarExtensions`](ProgressBarExtensions.md) | class | Fluent configuration for [`ProgressBar`](ProgressBar.md). |
| [`RadioButton`](RadioButton.md) | class | A radio button with a [`RadioButton.Value`](RadioButton.md). |
| [`RadioButtonExtensions`](RadioButtonExtensions.md) | class | Fluent configuration for [`RadioButton`](RadioButton.md). |
| [`RadioGroup`](RadioGroup.md) | class | A column of radio buttons of which at most one is selected. |
| [`RadioGroupExtensions`](RadioGroupExtensions.md) | class | Fluent configuration for [`RadioGroup`](RadioGroup.md). |
| [`SelectionArea`](SelectionArea.md) | class | A region whose text the user can select across nodes with one drag, as one selection. |
| [`Slider`](Slider.md) | class | Picks a value between [`Slider.Minimum`](Slider.md) and [`Slider.Maximum`](Slider.md), snapped to [`Slider.Step`](Slider.md), by pressing or dragging along the track, or with the arrow keys, Home and End while focused. |
| [`SliderColors`](SliderColors.md) | record | Colors a [`Slider`](Slider.md) uses instead of the theme's; unset colors follow the theme. |
| [`SliderExtensions`](SliderExtensions.md) | class | Fluent configuration for [`Slider`](Slider.md). |
| [`SliderPresenter`](SliderPresenter.md) | class | The track, fill and thumb a [`SliderTemplate`](SliderTemplate.md) builds inside a slider. |
| [`SliderSizing`](SliderSizing.md) | record | Sizes for a slider; unset sizes follow the default template. |
| [`SliderTemplate`](SliderTemplate.md) | class | Creates the parts of a [`Slider`](Slider.md) for its sizing, as WPF's control templates do. |
| [`SliderVisualState`](SliderVisualState.md) | record struct | The state a slider's presenter shows. |
| [`Switch`](Switch.md) | class | An on/off switch. |
| [`SwitchExtensions`](SwitchExtensions.md) | class | Fluent configuration for [`Switch`](Switch.md). |
| [`TabItem`](TabItem.md) | class | One tab of a [`TabView`](TabView.md): its label, description and enabled state, and its content, a retained view created the first time the tab is selected and kept until the tab is removed. |
| [`TabSelectionChangedEventArgs`](TabSelectionChangedEventArgs.md) | class | A tab view's selection change: the tab that was selected and the one that is, or -1 and null for none. |
| [`TabView`](TabView.md) | class | Shows the content of one of its tabs at a time, in [`TabView.ContentPresenter`](TabView.md). |
| [`TabViewExtensions`](TabViewExtensions.md) | class | Fluent configuration for [`TabView`](TabView.md) and [`TabItem`](TabItem.md). |
| [`TextArea`](TextArea.md) | class | A multi-line text field in a scroll box. |
| [`TextInput`](TextInput.md) | class | A single-line text field. |
| [`TextInputColors`](TextInputColors.md) | record | Colors a text input or text area uses instead of the theme's; unset colors follow the theme. |
| [`TextInputCore`](TextInputCore.md) | class | The editable text control that [`TextInput`](TextInput.md) and [`TextArea`](TextArea.md) build on: a shell styled by its [`TextInputTemplate`](TextInputTemplate.md) around an editable text node, with a placeholder while the text is empty. |
| [`TextInputExtensions`](TextInputExtensions.md) | class | Fluent configuration for [`TextInputCore`](TextInputCore.md) controls. |
| [`TextInputPresenter`](TextInputPresenter.md) | class | Styles a text input's shell for each state. |
| [`TextInputTemplate`](TextInputTemplate.md) | class | Creates a text input's presenter, as WPF's control templates do. |
| [`TextInputVisualState`](TextInputVisualState.md) | record struct | The state a text input's presenter styles: whether it is a multi-line area, enabled, and wrapping. |
| [`ToolTip`](ToolTip.md) | class | A text hint shown next to a node while the pointer rests on it, or while the node has keyboard focus. |
| [`ToolTipExtensions`](ToolTipExtensions.md) | class | Fluent tooltip assignment. |

## Nodes and layout

| Type | Kind | Summary |
|---|---|---|
| [`ContentContainerExtensions`](ContentContainerExtensions.md) | class | Fluent content for every [`IContentContainer`](IContentContainer.md). |
| [`CustomDrawable`](CustomDrawable.md) | class | A box that draws itself with a [`DrawContext`](DrawContext.md), as WPF elements do in `OnRender`. |
| [`FlexBox`](FlexBox.md) | class | A flexbox container with box styling: size, flex layout, padding and margin, background, border, corners, opacity, blur, shadow and gradient. |
| [`FlexBoxExtensions`](FlexBoxExtensions.md) | class | Fluent configuration for [`FlexBox`](FlexBox.md) and its subclasses. |
| [`Grid`](Grid.md) | class | A grid container. |
| [`GridExtensions`](GridExtensions.md) | class | Fluent configuration for [`Grid`](Grid.md) and for nodes placed in a grid. |
| [`GridLength`](GridLength.md) | record struct | The size of a grid row or column: pixels, a share of the remaining space (star), or sized to content. |
| [`IContentContainer`](IContentContainer.md) | interface | A node that takes content. |
| [`Image`](Image.md) | class | A texture loaded by the host from a URL, or shown by the id of a texture loaded elsewhere. |
| [`ISizable`](ISizable.md) | interface | A node sized by a fixed length, by filling the space its parent offers, and by min/max limits. |
| [`Portal`](Portal.md) | class | A flex box rendered above the normal tree and not clipped by its ancestors, for overlays such as popups. |
| [`ScrollBar`](ScrollBar.md) | class | A scroll bar for one axis of a [`ScrollState`](ScrollState.md): a track holding a thumb sized to the visible part of the content. |
| [`ScrollBarExtensions`](ScrollBarExtensions.md) | class | Fluent styling for [`ScrollBar`](ScrollBar.md), as a [`ScrollBox`](ScrollBox.md)'s bars are restyled. |
| [`ScrollBarVisibility`](ScrollBarVisibility.md) | enum | When a [`ScrollBox`](ScrollBox.md) shows a scroll bar, as WPF's ScrollViewer does. |
| [`ScrollBox`](ScrollBox.md) | class | A [`ScrollView`](ScrollView.md) with scroll bars, like WPF's ScrollViewer: content added to the box goes into the view, and each bar shows according to its [`ScrollBarVisibility`](ScrollBarVisibility.md). |
| [`ScrollBoxExtensions`](ScrollBoxExtensions.md) | class | Fluent configuration for [`ScrollBox`](ScrollBox.md). |
| [`ScrollChangedEventArgs`](ScrollChangedEventArgs.md) | class | A scroll view scrolling, or its content or viewport changing size. |
| [`ScrollState`](ScrollState.md) | class | The scroll position and sizes of a [`ScrollView`](ScrollView.md) as signals, shared with the scroll bars and other views that follow it. |
| [`ScrollView`](ScrollView.md) | class | A native scroll viewport over its content. |
| [`ScrollViewExtensions`](ScrollViewExtensions.md) | class | Fluent configuration for [`ScrollView`](ScrollView.md). |
| [`SizableExtensions`](SizableExtensions.md) | class | Fluent sizing for every [`ISizable`](ISizable.md) node or layout. |
| [`Svg`](Svg.md) | class | An SVG loaded by the host from a URL, or shown by the id of an SVG loaded elsewhere, optionally tinted. |
| [`VirtualList`](VirtualList.md) | class | A vertically scrolling list of fixed-height items that builds only the rows in view. |
| [`VirtualListExtensions`](VirtualListExtensions.md) | class | Fluent configuration for [`VirtualList`](VirtualList.md). |

## Text and fonts

| Type | Kind | Summary |
|---|---|---|
| [`DynamicTextLayout`](DynamicTextLayout.md) | class | Short, single-style text that changes often, such as counters and clocks. |
| [`DynamicTextOverflow`](DynamicTextOverflow.md) | enum | What a [`DynamicTextLayout`](DynamicTextLayout.md) does with text that has characters outside its charset. |
| [`FontFace`](FontFace.md) | record struct | A font the host has loaded or is loading, identified by its font id. |
| [`FontFaceLoadedEventArgs`](FontFaceLoadedEventArgs.md) | class | A font that finished loading. |
| [`FontFamily`](FontFamily.md) | class | The stacks of one family by weight and style; [`FontFamily.Resolve`](FontFamily.md) picks the closest face. |
| [`FontsLoadedEventArgs`](FontsLoadedEventArgs.md) | class | A set of fonts that finished loading. |
| [`FontStack`](FontStack.md) | class | A primary font with fallbacks for glyphs it lacks. |
| [`FontStackLoadedEventArgs`](FontStackLoadedEventArgs.md) | class | A font stack whose fonts finished loading. |
| [`FontStyle`](FontStyle.md) | enum | A font's slant. |
| [`FontWeight`](FontWeight.md) | enum | A font's weight, on the CSS scale. |
| [`ITextFormat`](ITextFormat.md) | interface | The text and its formatting, shared by text nodes and [`TextLayout`](TextLayout.md); one set of fluent members serves both. |
| [`RichText`](RichText.md) | class | Text made of styled [`Span`](Span.md)s, sent to the host as one string with style runs. |
| [`Span`](Span.md) | class | A run of rich text. |
| [`Text`](Text.md) | class | Display text: selectable by default, and announced as static text labelled with its content unless an ancestor already has a semantic role. |
| [`TextCore`](TextCore.md) | class | A retained text node: content, font, color, alignment, wrapping, limits, selection and editing. |
| [`TextExtensions`](TextExtensions.md) | class | Fluent configuration for text nodes, text layouts and rich-text spans. |
| [`TextLayout`](TextLayout.md) | class | Formatted text for drawing and measuring outside the page tree. |
| [`TextLayoutReadyEventArgs`](TextLayoutReadyEventArgs.md) | class | A text layout becoming ready to measure. |

## Drawing

| Type | Kind | Summary |
|---|---|---|
| [`Bitmap`](Bitmap.md) | class | An RGBA pixel buffer committed to a texture: write [`Bitmap.Pixels`](Bitmap.md), draw on [`Bitmap.Canvas`](Bitmap.md) or render text into it, then [`Bitmap.Commit`](Bitmap.md) uploads it under [`Bitmap.TextureId`](Bitmap.md) for an [`Image`](Image.md) or a draw call. |
| [`BitmapTextReadyEventArgs`](BitmapTextReadyEventArgs.md) | class | The node whose text [`Bitmap.WhenTextReady`](Bitmap.md) reports ready to render. |
| [`DrawContext`](DrawContext.md) | class | Immediate-mode drawing into a host canvas, handed to [`CustomDrawable.OnRender`](CustomDrawable.md). |
| [`Paint`](Paint.md) | record struct | How a shape is filled and stroked. |
| [`PathGeometry`](PathGeometry.md) | class | A vector path held by the host and drawn with [`DrawContext.DrawPath`](DrawContext.md). |

## Images and assets

| Type | Kind | Summary |
|---|---|---|
| [`Asset`](Asset.md) | class | A texture or SVG the host loads from a URL, for drawing or for showing by id. |
| [`AssetLoadState`](AssetLoadState.md) | enum | Where an asset's load stands. |
| [`IImageContent`](IImageContent.md) | interface | A node that shows a host-loaded asset: [`Image`](Image.md) or [`Svg`](Svg.md). |
| [`ImageContentExtensions`](ImageContentExtensions.md) | class | Fluent configuration for [`Image`](Image.md) and [`Svg`](Svg.md). |
| [`ImageSampling`](ImageSampling.md) | record struct | How a texture or SVG is sampled when it is scaled. |
| [`SvgAsset`](SvgAsset.md) | class | An SVG the host loads from a URL; see [`Asset`](Asset.md). |
| [`TextureAsset`](TextureAsset.md) | class | A texture the host loads from a URL; see [`Asset`](Asset.md). |

## Application, events and theming

| Type | Kind | Summary |
|---|---|---|
| [`Animation`](Animation.md) | class | A timed animation driven by the host's frames. |
| [`AnimationManager`](AnimationManager.md) | class | Runs animations on the host's frames. |
| [`AnimationTiming`](AnimationTiming.md) | class | How long an animation runs and how its progress eases; a negative duration is zero. |
| [`Application`](Application.md) | class | Mounts retained roots inside the application shell and flushes their commits to the host. |
| [`Application<TPage>`](Application-1.md) | class | An application whose page is a `TPage`, such as a controller that owns the root node. |
| [`ApplicationRegistration`](ApplicationRegistration.md) | class | Collects an application's settings for [`Application.Register`](Application.md). |
| [`Border`](Border.md) | record struct | A box outline; dashed and dotted borders alternate [`Border.DashOn`](Border.md) drawn and [`Border.DashOff`](Border.md) skipped pixels. |
| [`CheckboxChangedEventArgs`](CheckboxChangedEventArgs.md) | class | A checkbox changing state. |
| [`ClickEventArgs`](ClickEventArgs.md) | class | A button's click, which carries no data. |
| [`Color`](Color.md) | record struct | An RGBA color packed as 0xRRGGBBAA, the layout the host expects. |
| [`ComboBoxChangedEventArgs<TItem>`](ComboBoxChangedEventArgs-1.md) | class | A combo box's selection changing. |
| [`ContextMenuEventArgs`](ContextMenuEventArgs.md) | class | A request for a context menu over [`ContextMenuEventArgs.Target`](ContextMenuEventArgs.md) at a page point. |
| [`ContextMenuItemTheme`](ContextMenuItemTheme.md) | record | How a context menu draws its items. |
| [`ContextMenuManager`](ContextMenuManager.md) | class | Answers the host's context-menu requests: a node's [`Node.ContextMenuOpening`](Node.md) handler, or the application's built-in menu with the items that fit the target (copy and select all for text, opening an image, copying the page selection) and the page's history and reload items. |
| [`ContextMenuTheme`](ContextMenuTheme.md) | record | How a context menu's panel looks. |
| [`CornerRadius`](CornerRadius.md) | record struct | The radius of each corner of a box. |
| [`CubicInEasing`](CubicInEasing.md) | class | Starts slow and speeds up, as a cubic. |
| [`CubicInOutEasing`](CubicInOutEasing.md) | class | Slow at both ends, as a cubic. |
| [`CubicOutEasing`](CubicOutEasing.md) | class | Starts fast and slows down, as a cubic. |
| [`DragCompletedEventArgs`](DragCompletedEventArgs.md) | class | A drag finishing. |
| [`DragDataObject`](DragDataObject.md) | class | The data a drag source offers, keyed by format. |
| [`DragDeltaEventArgs`](DragDeltaEventArgs.md) | class | A drag moving. |
| [`DragDropEffects`](DragDropEffects.md) | enum | What a drop does with the dragged data, as the source allows and the target accepts. |
| [`DragEventArgs`](DragEventArgs.md) | class | A drag entering, moving over, leaving or dropping on a node. |
| [`DragGesture`](DragGesture.md) | class | Recognizes a pointer drag on `host`: captures the pointer on press, starts once the pointer moves past the threshold (or on long press), and reports deltas until release or cancel. |
| [`DragGestureCompletedEventArgs`](DragGestureCompletedEventArgs.md) | class | A drag ending. |
| [`DragSession`](DragSession.md) | class | A drag in progress from [`DragSession.Source`](DragSession.md). |
| [`DragStartedEventArgs`](DragStartedEventArgs.md) | class | A drag starting, once the pointer has moved past the threshold. |
| [`DropdownChangedEventArgs<TItem>`](DropdownChangedEventArgs-1.md) | class | A dropdown's selection changing. |
| [`DropProposal`](DropProposal.md) | record struct | A drop target's answer to a drag: the effect it would apply, and whether to show an insertion marker. |
| [`DropShadow`](DropShadow.md) | record struct | A shadow behind a box: its color, offset, blur and spread. |
| [`Easing`](Easing.md) | class | Maps linear progress from 0 to 1 to eased progress. |
| [`Easings`](Easings.md) | class | The built-in easings. |
| [`FocusChangedEventArgs`](FocusChangedEventArgs.md) | class | A node gaining or losing keyboard focus. |
| [`FrameScheduler`](FrameScheduler.md) | class | Coalesces retained mutations into one commit per frame, and runs loaded and after-commit callbacks. |
| [`GestureEventArgs`](GestureEventArgs.md) | class | Pan and pinch gesture events. |
| [`GestureEventKind`](GestureEventKind.md) | enum | The gesture a gesture event reports. |
| [`GestureEventPhase`](GestureEventPhase.md) | enum | Where a gesture event falls in its gesture. |
| [`GestureIntent`](GestureIntent.md) | enum | The touch gestures a node handles itself, so the host does not scroll or zoom the page for them. |
| [`GradientStop`](GradientStop.md) | record struct | A color at a position along a gradient, from 0 to 1. |
| [`GridCell`](GridCell.md) | record struct | A child's cell in its parent grid; a new cell is the first row and column, spanning one of each. |
| [`HoverChangedEventArgs`](HoverChangedEventArgs.md) | class | The pointer entering or leaving a node. |
| [`IApplication`](IApplication.md) | interface | A runnable application: [`IApplication.Run`](IApplication.md) builds and mounts its page, `IDisposable.Dispose` unmounts it. |
| [`Keyboard`](Keyboard.md) | class | Keyboard focus, as WPF's `Keyboard` reports it. |
| [`KeyEventArgs`](KeyEventArgs.md) | class | A key pressed or released while a node has focus. |
| [`Length`](Length.md) | record struct | A length in pixels or percent of the parent, or [`Length.Auto`](Length.md). |
| [`LinearEasing`](LinearEasing.md) | class | No easing: progress is linear. |
| [`LinearGradient`](LinearGradient.md) | class | A gradient from ([`LinearGradient.StartX`](LinearGradient.md), [`LinearGradient.StartY`](LinearGradient.md)) to ([`LinearGradient.EndX`](LinearGradient.md), [`LinearGradient.EndY`](LinearGradient.md)) through [`LinearGradient.Stops`](LinearGradient.md). |
| [`LoadedEventArgs`](LoadedEventArgs.md) | class | The page's first frame being committed. |
| [`Logger`](Logger.md) | class | Categorized logging through the host. |
| [`LongPressDefaults`](LongPressDefaults.md) | class | The defaults a long press is recognized with. |
| [`LongPressEventArgs`](LongPressEventArgs.md) | class | A long press: a pointer held still past the minimum duration. |
| [`LongPressGesture`](LongPressGesture.md) | class | A configurable long-press recognizer for [`Node`](Node.md). |
| [`ManagedApplicationController`](ManagedApplicationController.md) | class | A page that mounts and unmounts itself; [`Application.CreateManaged`](Application.md) runs it. |
| [`Node`](Node.md) | class | A retained UI node. |
| [`NodeExtensions`](NodeExtensions.md) | class | Fluent configuration for every node. |
| [`NodeTransitions`](NodeTransitions.md) | class | Which property changes on a built node animate, and how. |
| [`Point`](Point.md) | record struct | A point in scene pixels. |
| [`PointerButton`](PointerButton.md) | enum | The button that changed in a pointer event, numbered as the browser numbers them. |
| [`PointerButtons`](PointerButtons.md) | enum | The buttons held during a pointer event, as flags. |
| [`PointerClickEventArgs`](PointerClickEventArgs.md) | class | A click: a primary press and release on the same node. |
| [`PointerEventArgs`](PointerEventArgs.md) | class | A pointer moving, pressing, releasing, entering, leaving or being cancelled over a node. |
| [`PointerType`](PointerType.md) | enum | The kind of device behind a pointer event. |
| [`QuadOutEasing`](QuadOutEasing.md) | class | Starts fast and slows down, as a quadratic. |
| [`RadioButtonChangedEventArgs`](RadioButtonChangedEventArgs.md) | class | A radio button being selected or deselected. |
| [`RadioGroupChangedEventArgs`](RadioGroupChangedEventArgs.md) | class | A radio group's selection changing. |
| [`Rect`](Rect.md) | record struct | A rectangle in scene pixels. |
| [`RetainedView`](RetainedView.md) | class | A retained subtree that can be activated, deactivated and reused, such as the content of a tab. |
| [`SelectionChangedEventArgs`](SelectionChangedEventArgs.md) | class | A selection changing. |
| [`SelectionHandleMode`](SelectionHandleMode.md) | enum | When text selections show draggable handles. |
| [`SelectionHandles`](SelectionHandles.md) | class | The draggable handles at the two ends of a text selection, for touch. |
| [`SemanticValueRange`](SemanticValueRange.md) | record struct | An accessibility value range: the current value and its bounds. |
| [`Signal<T>`](Signal-1.md) | class | A value that raises [`Signal.Changed`](Signal-1.md) whenever it is set to a different value. |
| [`SliderChangedEventArgs`](SliderChangedEventArgs.md) | class | A slider's value changing. |
| [`SwitchChangedEventArgs`](SwitchChangedEventArgs.md) | class | A switch turning on or off. |
| [`TextChangedEventArgs`](TextChangedEventArgs.md) | class | A text value changing. |
| [`TextMetrics`](TextMetrics.md) | record struct | Measured text layout: size in pixels, first-line baseline, line count and widest line. |
| [`Theme`](Theme.md) | record | Colors, spacing, fonts and component styles. |
| [`ThemeColors`](ThemeColors.md) | record | The theme's palette. |
| [`ThemeFonts`](ThemeFonts.md) | record | The theme's font stacks and families; families default to regular/bold pairs of the stacks. |
| [`ThemeSpacing`](ThemeSpacing.md) | record | The theme's spacing steps, extra small to extra large, in pixels. |
| [`Thickness`](Thickness.md) | record struct | Space on each side of a box, as for padding and margin. |
| [`ToolTipTheme`](ToolTipTheme.md) | record | How a tooltip looks. |
| [`VisibilityChangedEventArgs`](VisibilityChangedEventArgs.md) | class | A popup or menu showing or hiding. |
| [`WheelDeltaMode`](WheelDeltaMode.md) | enum | The unit of a wheel event's deltas. |
| [`WheelEventArgs`](WheelEventArgs.md) | class | A mouse wheel or trackpad scroll over a node. |

## Platform, persistence, fetch and workers

| Type | Kind | Summary |
|---|---|---|
| [`FetchException`](FetchException.md) | class | The host could not complete a [`FetchRequest`](FetchRequest.md) (a network failure or a blocked request). |
| [`FetchRequest`](FetchRequest.md) | class | An HTTP request sent through the host. |
| [`FetchRequestExtensions`](FetchRequestExtensions.md) | class | Fluent configuration for [`FetchRequest`](FetchRequest.md). |
| [`FetchResponse`](FetchResponse.md) | record | The host's answer to a [`FetchRequest`](FetchRequest.md): its status and final URL. |
| [`HostContext`](HostContext.md) | record | The host the application runs in: its platform family, environment and capabilities. |
| [`KeyboardShortcuts`](KeyboardShortcuts.md) | class | The platform's keyboard conventions: which modifier drives shortcuts and text navigation, undo and redo detection, and shortcut labels. |
| [`Navigation`](Navigation.md) | class | The host's current route and navigation. |
| [`PersistedCodec<TValue>`](PersistedCodec-1.md) | record | Converts a persisted value to and from its saved text. |
| [`PersistedCodecs`](PersistedCodecs.md) | class | Codecs for common values, in the invariant culture. |
| [`PersistedStateAdapter`](PersistedStateAdapter.md) | class | Saves part of a node's state into the host's per-history-entry UI state and restores it when the user returns, for nodes with a [`Node.NodeId`](Node.md). |
| [`PersistedValue<TNode, TValue>`](PersistedValue-2.md) | class | Persists one value of a node, read with `capture` and written back with `restore`. |
| [`Platform`](Platform.md) | class | Queries about the host, read once per runtime. |
| [`Worker`](Worker.md) | class | A one-shot background job in a Web Worker. |
| [`WorkerCompletedEventArgs`](WorkerCompletedEventArgs.md) | class | A worker finishing. |
| [`WorkerErrorEventArgs`](WorkerErrorEventArgs.md) | class | A worker failing. |
| [`WorkerExtensions`](WorkerExtensions.md) | class | Fluent configuration for [`Worker`](Worker.md). |
| [`WorkerProgressEventArgs`](WorkerProgressEventArgs.md) | class | A progress message from a worker. |

## Worker projects (FuiCs.Worker)

| Type | Kind | Summary |
|---|---|---|
| [`FetchException`](Workers.FetchException.md) | class | The worker could not complete a [`FetchRequest`](Workers.FetchRequest.md) (a network failure or a blocked request). |
| [`FetchRequest`](Workers.FetchRequest.md) | class | An HTTP request a worker job sends through its Web Worker, as the page's `FetchRequest` does; a relative URL resolves against the worker's script. |
| [`FetchRequestExtensions`](Workers.FetchRequestExtensions.md) | class | Fluent configuration for [`FetchRequest`](Workers.FetchRequest.md). |
| [`FetchResponse`](Workers.FetchResponse.md) | record | The host's answer to a worker's [`FetchRequest`](Workers.FetchRequest.md): its status and final URL, without the body. |
| [`IWorkerHost`](Workers.IWorkerHost.md) | interface | The worker's side of the page link, which the worker project's generated exports provide. |
| [`WorkerEntries`](Workers.WorkerEntries.md) | class | The named entries a worker project offers; the page starts one with `new Worker(path, "name")`. |
| [`WorkerHosting`](Workers.Hosting.WorkerHosting.md) | class | Runs the worker's entries for its generated exports. |
| [`WorkerJob`](Workers.WorkerJob.md) | class | A worker job with state: [`WorkerJob.OnStart`](Workers.WorkerJob.md) receives the input, then [`WorkerJob.Run`](Workers.WorkerJob.md) does the work, awaiting [`WorkerJob.YieldAsync`](Workers.WorkerJob.md) between slices. |
| [`WorkerJobException`](Workers.WorkerJobException.md) | class | A worker job's failure, with the message the job gave; the page receives it as the worker's error. |
| [`WorkerRuntime`](Workers.WorkerRuntime.md) | class | The running worker job's link to the page: progress reports, then one result or failure. |

## Enumerations shared with the runtime

| Type | Kind | Summary |
|---|---|---|
| [`AbiCall`](Testing.AbiCall.md) | record | One recorded ABI call: the ABI function name and its value-level arguments. |
| [`AlignItems`](AlignItems.md) | enum |  |
| [`AlignSelf`](AlignSelf.md) | enum |  |
| [`BorderStyle`](BorderStyle.md) | enum |  |
| [`CursorStyle`](CursorStyle.md) | enum |  |
| [`FlexDirection`](FlexDirection.md) | enum |  |
| [`FlexWrap`](FlexWrap.md) | enum |  |
| [`GridUnit`](GridUnit.md) | enum |  |
| [`HandleValue`](HandleValue.md) | enum |  |
| [`HostCapability`](HostCapability.md) | enum |  |
| [`HostEnvironment`](HostEnvironment.md) | enum |  |
| [`IFuiAbi`](IFuiAbi.md) | interface | The EffinDOM ABI calls fui-cs makes, at value level. |
| [`ImageSamplingKind`](ImageSamplingKind.md) | enum |  |
| [`JustifyContent`](JustifyContent.md) | enum |  |
| [`KeyEventType`](KeyEventType.md) | enum |  |
| [`KeyModifier`](KeyModifier.md) | enum |  |
| [`NodeType`](NodeType.md) | enum |  |
| [`ObjectFit`](ObjectFit.md) | enum |  |
| [`Orientation`](Orientation.md) | enum |  |
| [`PlatformFamily`](PlatformFamily.md) | enum |  |
| [`PointerEventType`](PointerEventType.md) | enum |  |
| [`PositionType`](PositionType.md) | enum |  |
| [`RecordingFuiAbi`](Testing.RecordingFuiAbi.md) | class | Records every ABI call instead of reaching EffinDOM, for tests on desktop .NET and NetWasm. |
| [`SemanticCheckedState`](SemanticCheckedState.md) | enum |  |
| [`SemanticRole`](SemanticRole.md) | enum |  |
| [`TextAlign`](TextAlign.md) | enum |  |
| [`TextOverflow`](TextOverflow.md) | enum |  |
| [`TextVerticalAlign`](TextVerticalAlign.md) | enum |  |
| [`Unit`](Unit.md) | enum |  |
| [`Visibility`](Visibility.md) | enum |  |

## Host entry points

| Type | Kind | Summary |
|---|---|---|
| [`HostEvents`](Hosting.HostEvents.md) | class | Entry points for the host: application lifecycle, frames and input. |
