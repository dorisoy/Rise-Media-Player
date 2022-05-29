﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using Rise.App.ViewModels;
using Rise.Data.ViewModels;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Rise.App.Views
{
    /// <summary>
    /// A page that shows the current state of playback.
    /// </summary>
    public sealed partial class NowPlayingPage : Page
    {
        private MediaPlaybackViewModel MPViewModel => App.MPViewModel;
        private SettingsViewModel SViewModel => App.SViewModel;

        public NowPlayingPage()
        {
            InitializeComponent();

            if (MPViewModel.PlayerCreated)
                MainPlayer.SetMediaPlayer(MPViewModel.Player);
            else
                MPViewModel.MediaPlayerRecreated += OnMediaPlayerRecreated;

            Debug.Assert(ApplyVisualizer(SViewModel.VisualizerType));
            Debug.Assert(ApplyMode(SViewModel.NowPlayingMode));

            // No need for pointer in events when we're in the main window
            if (SViewModel.NowPlayingMode == 1)
            {
                VisualStateManager.GoToState(this, "PointerInState", true);
            }
            else
            {
                PointerEntered += OnPointerEntered;
                PointerExited += OnPointerExited;
                PointerCanceled += OnPointerExited;
            }

            if (SViewModel.VisualizerType == 1 && SViewModel.NowPlayingMode != 1)
            {
                VisualStateManager.GoToState(this, "NoVisualizerState", true);
            }

            SViewModel.PropertyChanged += OnSettingChanged;
            Unloaded += OnPageUnloaded;
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            PointerEntered -= OnPointerEntered;
            PointerExited -= OnPointerExited;
            PointerCanceled -= OnPointerExited;

            MPViewModel.MediaPlayerRecreated -= OnMediaPlayerRecreated;
            SViewModel.PropertyChanged -= OnSettingChanged;
        }
    }

    // Event handlers
    public sealed partial class NowPlayingPage
    {
        // Buttons
        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        private async void OnExitOverlayClick(object sender, RoutedEventArgs e)
        {
            _ = await ApplicationView.GetForCurrentView().
                TryEnterViewModeAsync(ApplicationViewMode.Default, ViewModePreferences.CreateDefault(ApplicationViewMode.Default));
            if (Frame.CanGoBack) Frame.GoBack();
        }

        // Media playback
        private async void OnMediaPlayerRecreated(object sender, Windows.Media.Playback.MediaPlayer e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MainPlayer.SetMediaPlayer(e));
        }

        private void OnShufflingChanged(object sender, bool e)
            => MPViewModel.ShuffleEnabled = e;

        // UI
        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerInState", true);

            if (SViewModel.VisualizerType == 1)
            {
                VisualStateManager.GoToState(this, "LineVisualizerState", true);
            }
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOutState", true);

            if (SViewModel.VisualizerType == 1)
            {
                VisualStateManager.GoToState(this, "NoVisualizerState", true);
            }
        }

        // Settings
        private void OnSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SViewModel.VisualizerType):
                    Debug.Assert(ApplyVisualizer(SViewModel.VisualizerType));
                    break;
            }
        }

        private bool ApplyMode(int index) => index switch
        {
            1 => VisualStateManager.GoToState(this, "MainWindowState", true),
            2 => VisualStateManager.GoToState(this, "CompactOverlayState", true),
            _ => VisualStateManager.GoToState(this, "SeparateWindowState", true),
        };

        private bool ApplyVisualizer(int index) => index switch
        {
            1 => VisualStateManager.GoToState(this, "LineVisualizerState", false),
            2 => VisualStateManager.GoToState(this, "BloomVisualizerState", false),
            _ => VisualStateManager.GoToState(this, "NoVisualizerState", false),
        };
    }
}
