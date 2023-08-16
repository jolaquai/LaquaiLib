using System.Windows;
using System.Windows.Media;

using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using UserControl = System.Windows.Controls.UserControl;

namespace LaquaiLib.Extensions.WPF.Controls;

/// <summary>
/// Represents a progress bar that indicates the progress or status of a process.
/// </summary>
public partial class ProgressBar : UserControl
{
    #region Enums
    /// <summary>
    /// Identifies the layout style and orientation of a <see cref="ProgressBar"/>.
    /// </summary>
    public enum Orientations
    {
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> is oriented horizontally.
        /// </summary>
        Horizontal,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> is oriented vertically.
        /// </summary>
        Vertical,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> is oriented circularly.
        /// </summary>
        Circular
    }
    /// <summary>
    /// Identifies the style of a <see cref="ProgressBar"/>.
    /// </summary>
    public enum Styles
    {
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> displays progress by filling its background with its primary color.
        /// </summary>
        Solid,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> displays progress by filling its background with its primary color while animating blocks of its secondary color from its beginning to its end.
        /// </summary>
        Pulsing,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> displays progress by filling its background with a dashed pattern; that is, assuming a horizontal orientation, it fills its background with alternating vertical lines and empty space.
        /// </summary>
        Dashes,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> displays progress by filling its background with alternating blocks of color and empty space.
        /// </summary>
        Blocks,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> displays progress by filling its background with alternating rounded blocks of color and empty space.
        /// </summary>
        Rounded,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> displays progress by filling its background with alternating dots of color and empty space.
        /// </summary>
        Dots
    }
    /// <summary>
    /// Identifies the state of a <see cref="ProgressBar"/>.
    /// </summary>
    public enum States
    {
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> is in its default state; that is, it is able to display progress between its configured <see cref="Minimum"/> and <see cref="Maximum"/> values normally.
        /// </summary>
        Default,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> is frozen; that is, changes made to its <see cref="Value"/> are not reflected in its appearance.
        /// </summary>
        Frozen,
        /// <summary>
        /// Indicates that the <see cref="ProgressBar"/> is in an indeterminate state; that is, it ignores its <see cref="Value"/> and displays an animation representing an ongoing process instead of concrete progress.
        /// </summary>
        Indeterminate
    }
    #endregion

    #region Constants
    private const bool isReverseDefault = false;
    #endregion

    #region Dependency Properties
    /// <summary>
    /// Identifies the <see cref="MaximumProperty"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(ProgressBar), new PropertyMetadata(100));
    /// <summary>
    /// Identifies the <see cref="MinimumProperty"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(ProgressBar), new PropertyMetadata(0));
    /// <summary>
    /// Identifies the <see cref="ValueProperty"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(ProgressBar), MinimumProperty.GetMetadata(typeof(ProgressBar)));
    /// <summary>
    /// Identifies the <see cref="StepProperty"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(Step), typeof(double), typeof(ProgressBar), new PropertyMetadata(1.0));
    /// <summary>
    /// Identifies the <see cref="BarOrientationProperty"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BarOrientationProperty = DependencyProperty.Register(nameof(BarOrientation), typeof(Orientations), typeof(ProgressBar), new PropertyMetadata(Orientations.Horizontal));
    /// <summary>
    /// Identifies the <see cref="IsReverseProperty"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsReverseProperty = DependencyProperty.Register(nameof(IsReverse), typeof(bool), typeof(ProgressBar), new PropertyMetadata(isReverseDefault));
    public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register(nameof(BarStyle), typeof(Styles), typeof(ProgressBar), new PropertyMetadata(Styles.Solid));
    public static readonly DependencyProperty BarStateProperty = DependencyProperty.Register(nameof(BarState), typeof(States), typeof(ProgressBar), new PropertyMetadata(States.Default));
    public static readonly DependencyProperty PrimaryColorProperty = DependencyProperty.Register(nameof(PrimaryColor), typeof(Brush), typeof(ProgressBar), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xB2, 0x20))));
    public static readonly DependencyProperty SecondaryColorProperty = DependencyProperty.Register(nameof(SecondaryColor), typeof(Brush), typeof(ProgressBar), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7A, 0x12))));
    #endregion

    #region Fields/Properties
    private double _initial;
    private States? _previousState = null;

    /// <summary>
    /// The minimum value this <see cref="ProgressBar"/> can have.
    /// When <see cref="Value"/> equals this value, the <see cref="ProgressBar"/> is empty.
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    /// <summary>
    /// The maximum value this <see cref="ProgressBar"/> can have.
    /// When <see cref="Value"/> equals this value, the <see cref="ProgressBar"/> is full.
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    /// <summary>
    /// The current value of this <see cref="ProgressBar"/>.
    /// Values outside the range of <see cref="Minimum"/> and <see cref="Maximum"/> are clamped to said range.
    /// </summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, double.Clamp(Value, Minimum, Maximum));
    }
    /// <summary>
    /// The current value of this <see cref="ProgressBar"/>.
    /// </summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"The {nameof(Step)} value must be greater than zero.");
            }
            SetValue(StepProperty, value);
        }
    }
    /// <summary>
    /// The orientation and style of this <see cref="ProgressBar"/>.
    /// </summary>
    public Orientations BarOrientation
    {
        get => (Orientations)GetValue(BarOrientationProperty);
        set => SetValue(BarOrientationProperty, value);
    }
    /// <summary>
    /// Whether the progress bar is displayed in reverse, irrespective of its <see cref="Orientations"/>.
    /// </summary>
    public bool IsReverse
    {
        get => (bool)GetValue(IsReverseProperty);
        set => SetValue(IsReverseProperty, value);
    }
    public Styles BarStyle
    {
        get => (Styles)GetValue(BarStyleProperty);
        set => SetValue(BarStyleProperty, value);
    }
    public States BarState
    {
        get => (States)GetValue(BarStateProperty);
        set => SetValue(BarStateProperty, value);
    }
    public Brush PrimaryColor
    {
        get => (Brush)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }
    public Brush SecondaryColor
    {
        get => (Brush)GetValue(SecondaryColorProperty);
        set => SetValue(SecondaryColorProperty, value);
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Instantiates a new <see cref="ProgressBar"/>.
    /// </summary>
    public ProgressBar()
    {
        InitializeComponent();
    }
    /// <summary>
    /// Instantiates a new <see cref="ProgressBar"/> with the specified inclusive <paramref name="maximum"/> value.
    /// </summary>
    /// <param name="maximum">The inclusive maximum value the <see cref="ProgressBar"/> may assume.</param>
    public ProgressBar(double maximum) : this()
    {
        Maximum = maximum;
    }
    /// <summary>
    /// Instantiates a new <see cref="ProgressBar"/> with the specified inclusive <paramref name="minimum"/> and <paramref name="maximum"/> values.
    /// </summary>
    /// <param name="minimum">The inclusive minimum value the <see cref="ProgressBar"/> may assume.</param>
    /// <param name="maximum">The inclusive maximum value the <see cref="ProgressBar"/> may assume.</param>
    public ProgressBar(
        double minimum,
        double maximum
    ) : this(maximum)
    {
        _initial = minimum;
        Minimum = _initial;
    }
    /// <summary>
    /// Instantiates a new <see cref="ProgressBar"/> with the specified inclusive <paramref name="minimum"/> and <paramref name="maximum"/> values, and the specified <paramref name="step"/> value, optionally setting a specified <paramref name="orientation"/> and <paramref name="isReverse"/> value.
    /// </summary>
    /// <param name="minimum">The inclusive minimum value the <see cref="ProgressBar"/> may assume.</param>
    /// <param name="maximum">The inclusive maximum value the <see cref="ProgressBar"/> may assume.</param>
    /// <param name="step">The value by which the <see cref="Value"/> property is incremented or decremented when using the <see cref="Increment"/> and <see cref="Decrement"/> methods.</param>
    /// <param name="orientation">The <see cref="Orientations"/> of the <see cref="ProgressBar"/>.</param>
    /// <param name="isReverse">Whether the progress bar is displayed in reverse, irrespective of its <see cref="Orientations"/>.</param>
    public ProgressBar(
        double minimum,
        double maximum,
        double step,
        Orientations orientation = default,
        bool isReverse = isReverseDefault
    ) : this(minimum, maximum)
    {
        Step = step;
        BarOrientation = orientation;
        IsReverse = isReverse;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Increments the current <see cref="Value"/> by the configured <see cref="Step"/> value.
    /// </summary>
    /// <returns>The new <see cref="Value"/>.</returns>
    public double Increment() => Value += Step;
    /// <summary>
    /// Decrements the current <see cref="Value"/> by the configured <see cref="Step"/> value.
    /// </summary>
    /// <returns>The new <see cref="Value"/>.</returns>
    public double Decrement() => Value -= Step;

    /// <summary>
    /// Sets a new <see cref="Value"/> for this <see cref="ProgressBar"/>, irrespective of the configured <see cref="Step"/> value.
    /// </summary>
    /// <param name="newProgress">The new <see cref="Value"/> to set.</param>
    /// <returns>The new <see cref="Value"/>.</returns>
    public double SetProgress(double newProgress) => Value = newProgress;
    /// <summary>
    /// Resets the <see cref="Value"/> of this <see cref="ProgressBar"/> to its initial value (which is equal to <see cref="Minimum"/> if it has not been modified after instantiation).
    /// </summary>
    /// <returns>The new <see cref="Value"/>.</returns>
    public double ResetProgress() => Value = _initial;
    /// <summary>
    /// Resets the <see cref="Value"/> of this <see cref="ProgressBar"/> to the specified <paramref name="newMinimum"/> value and overwrites the old initial value, so that future calls to <see cref="ResetProgress()"/> will reset to this new value.
    /// </summary>
    /// <param name="newMinimum">The new <see cref="Minimum"/> and initial <see cref="Value"/> to set.</param>
    /// <returns>The new <see cref="Value"/>.</returns>
    public double ResetProgress(double newMinimum)
    {
        _initial = newMinimum;
        return ResetProgress();
    }

    /// <summary>
    /// Freezes the <see cref="ProgressBar"/> so that changes to its <see cref="Value"/> property are not reflected in its appearance.
    /// </summary>
    public void Freeze()
    {
        _previousState = BarState;
        BarState = States.Frozen;
    }
    /// <summary>
    /// Unfreezes the <see cref="ProgressBar"/> and restores its previous <see cref="BarState"/> value, if it was frozen.
    /// </summary>
    public void Unfreeze()
    {
        if (BarState == States.Frozen && _previousState is States state)
        {
            BarState = state;
            _previousState = null;
        }
    }
    #endregion

    #region Private Methods
    #endregion
}
