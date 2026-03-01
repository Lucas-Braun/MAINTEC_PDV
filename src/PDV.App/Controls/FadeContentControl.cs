using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PDV.App.Controls;

public class FadeContentControl : ContentControl
{
    private readonly TranslateTransform _translate;

    public FadeContentControl()
    {
        _translate = new TranslateTransform();
        RenderTransform = _translate;
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (newContent == null) return;

        var ease = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(220);

        // Fade in
        var fadeIn = new DoubleAnimation(0, 1, duration) { EasingFunction = ease };
        BeginAnimation(OpacityProperty, fadeIn);

        // Slide from right (30px)
        var slideIn = new DoubleAnimation(30, 0, duration) { EasingFunction = ease };
        _translate.BeginAnimation(TranslateTransform.XProperty, slideIn);
    }
}
