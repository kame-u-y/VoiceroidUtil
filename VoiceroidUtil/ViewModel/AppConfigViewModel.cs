using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RucheHome.AviUtl;
using RucheHome.AviUtl.ExEdit;
using RucheHome.Windows.Mvvm.Commands;
using VoiceroidUtil.Extensions;
using VoiceroidUtil.Services;
using static RucheHome.Util.ArgumentValidater;

namespace VoiceroidUtil.ViewModel
{
    /// <summary>
    /// アプリ設定とそれに対する処理を提供する ViewModel クラス。
    /// </summary>
    public class AppConfigViewModel : ConfigViewModelBase<AppConfig>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public AppConfigViewModel(
            IReadOnlyReactiveProperty<bool> canModify,
            IReadOnlyReactiveProperty<AppConfig> config,
            IReadOnlyReactiveProperty<UIConfig> uiConfig,
            IReactiveProperty<IAppStatus> lastStatus,
            IOpenFileDialogService openFileDialogService)
            : base(canModify, config)
        {
            ValidateArgumentNull(uiConfig, nameof(uiConfig));
            ValidateArgumentNull(lastStatus, nameof(lastStatus));
            ValidateArgumentNull(openFileDialogService, nameof(openFileDialogService));

            this.LastStatus = lastStatus;
            this.OpenFileDialogService = openFileDialogService;

            // 選択中タブインデックス
            this.SelectedTabIndex =
                this.MakeInnerPropertyOf(uiConfig, c => c.AppConfigTabIndex);

            // YmmCharaRelation コレクション
            var ymmCharaRelations =
                this.MakeReadOnlyConfigProperty(
                    c => c.YmmCharaRelations,
                    notifyOnSameValue: true);

            // 表示状態の YmmCharaRelation コレクション
            this.VisibleYmmCharaRelations =
                Observable
                    .CombineLatest(
                        this.ObserveConfigProperty(c => c.VoiceroidVisibilities),
                        ymmCharaRelations.Select(r => r.Count()).DistinctUntilChanged(),
                        (vv, _) => vv.SelectVisibleOf(ymmCharaRelations.Value))
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this.CompositeDisposable);

            // AviUtlDropLayer コレクション
            var aviUtlDropLayers =
                this.MakeReadOnlyConfigProperty(
                    c => c.AviUtlDropLayers,
                    notifyOnSameValue: true);

            // 表示状態の AviUtlDropLayer コレクション
            this.VisibleAviUtlDropLayers =
                Observable
                    .CombineLatest(
                        this.ObserveConfigProperty(c => c.VoiceroidVisibilities),
                        aviUtlDropLayers.Select(r => r.Count()).DistinctUntilChanged(),
                        (vv, _) => vv.SelectVisibleOf(aviUtlDropLayers.Value))
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this.CompositeDisposable);

            // .exo ファイル作成設定有効化コマンド表示状態
            this.IsExoFileMakingCommandInvisible =
                this.MakeReadOnlyConfigProperty(c => c.IsExoFileMaking);
            this.IsExoFileMakingCommandVisible =
                this.IsExoFileMakingCommandInvisible
                    .Inverse()
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this.CompositeDisposable);

            // 保存先ディレクトリ選択コマンド
            this.SelectSaveDirectoryCommand =
                this.MakeAsyncCommand(
                    this.ExecuteSelectSaveDirectoryCommand,
                    this.CanModify);

            // 保存先ディレクトリオープンコマンド
            this.OpenSaveDirectoryCommand =
                this.MakeAsyncCommand(this.ExecuteOpenSaveDirectoryCommand);

            // 保存先ディレクトリドラッグオーバーコマンド
            this.DragOverSaveDirectoryCommand =
                this.MakeCommand<DragEventArgs>(
                    this.ExecuteDragOverSaveDirectoryCommand,
                    this.CanModify);

            // 保存先ディレクトリドロップコマンド
            this.DropSaveDirectoryCommand =
                this.MakeCommand<DragEventArgs>(
                    this.ExecuteDropSaveDirectoryCommand,
                    this.CanModify);

            // .exo ファイル作成設定有効化コマンド
            this.ExoFileMakingCommand =
                this.MakeCommand(
                    this.ExecuteExoFileMakingCommand,
                    this.CanModify,
                    this.IsExoFileMakingCommandVisible);



            var charaStyles = this.MakeInnerReadOnlyPropertyOf(config, c => c.PreviewCharaStyles);
            this.VisibleCharaStyles = 
                Observable
                    .CombineLatest(
                        config.ObserveInnerProperty(c => c.VoiceroidVisibilities),
                        charaStyles.Select(s => s.Count()).DistinctUntilChanged(),
                        (vv, _) => vv.SelectVisibleOf(charaStyles.Value))
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this.CompositeDisposable);

            this.SelectedCharaStyle =
                new ReactiveProperty<PreviewCharaStyle>(this.VisibleCharaStyles.Value.First())
                    .AddTo(this.CompositeDisposable);
            

            this.IsSelectCharaStyleCommandExecutable =
                this.VisibleCharaStyles
                    .Select(vcs => vcs.Count > 2)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this.CompositeDisposable);

            this.SelectCharaStyleCommandTip =
                this.VisibleCharaStyles
                    .Select(_ => this.MakeSelectCharaStyleCommandTip())
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this.CompositeDisposable);

            this.VisibleCharaStylesColumnCount =
                this.VisibleCharaStyles
                    .Select(vp => Math.Min(Math.Max(1, (vp.Count + 5) / 6), 3))
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this.CompositeDisposable);

            this.Text = this.MakeInnerReadOnlyPropertyOf(this.SelectedCharaStyle, c => c.Text);
            this.Render = this.MakeInnerReadOnlyPropertyOf(this.SelectedCharaStyle, c => c.Render);

            this.SetupUIConfig(uiConfig);

            //this.SelectedFontFamilyName =
            //    new ReactiveProperty<string>(@"").AddTo(this.CompositeDisposable);

            //this.FontColor =
            //    new ReactiveProperty<Color>(Colors.Black).AddTo(this.CompositeDisposable);

            this.X = this.MakeMovableValueViewModel(this.Render, r => r.X);
            this.Y = this.MakeMovableValueViewModel(this.Render, r => r.Y);
            this.Z = this.MakeMovableValueViewModel(this.Render, r => r.Z);
            this.Scale = this.MakeMovableValueViewModel(this.Render, r => r.Scale);
            this.FontSize = this.MakeMovableValueViewModel(this.Text, t => t.FontSize);

        }

        public IReadOnlyReactiveProperty<ReadOnlyCollection<PreviewCharaStyle>>
            VisibleCharaStyles
            {
                get;
            }

        public ReactiveProperty<PreviewCharaStyle> SelectedCharaStyle { get; }

        public IReadOnlyReactiveProperty<string> SelectCharaStyleCommandTip { get; }

        public IReadOnlyReactiveProperty<bool> IsSelectCharaStyleCommandExecutable { get; }

        public IReadOnlyReactiveProperty<int> VisibleCharaStylesColumnCount { get; }

        /// <summary>
        /// キャラ別スタイル設定選択コマンドのチップテキストを作成する。
        /// </summary>
        /// <returns>チップテキスト。表示不要ならば null 。</returns>
        private string MakeSelectCharaStyleCommandTip() =>
            !this.IsSelectCharaStyleCommandExecutable.Value ?
                null :
                @"F1/F2 : 前/次のキャラを選択" + Environment.NewLine +
                string.Join(
                    Environment.NewLine,
                    this.VisibleCharaStyles.Value
                        .Take(10)
                        .Select(
                            (p, i) =>
                                @"Ctrl+" + ((i < 9) ? (i + 1) : 0) + @" : " +
                                p.VoiceroidName + @" を選択"));

        public IEnumerable<string> FontFamilyNames { get; } = new FontFamilyNameEnumerable();

        public IReadOnlyReactiveProperty<RenderComponent> Render { get; }
        public IReadOnlyReactiveProperty<TextComponent> Text { get; }
        //public IReadOnlyReactiveProperty<string> SelectedFontFamilyName { get; }

        //public IReadOnlyReactiveProperty<Color> FontColor { get; }

        public MovableValueViewModel X { get; private set; }
        public MovableValueViewModel Y { get; private set; }
        public MovableValueViewModel Z { get; private set; }
        public MovableValueViewModel Scale { get; private set; }
        public MovableValueViewModel FontSize { get; private set; }

        private MovableValueViewModel MakeMovableValueViewModel<T, TConstants>(
            IReadOnlyReactiveProperty<T> holder,
            Expression<Func<T, MovableValue<TConstants>>> selector,
            string name = null)
            where T : INotifyPropertyChanged
            where TConstants : IMovableValueConstants, new()
        {
            Debug.Assert(holder != null);
            Debug.Assert(selector != null);

            // 値取得
            var value = this.MakeInnerPropertyOf(holder, selector, this.CanModify);

            // 名前取得
            var info = ((MemberExpression)selector.Body).Member;
            name =
                name ??
                info.GetCustomAttribute<ExoFileItemAttribute>(true)?.Name ??
                info.Name;

            return
                new MovableValueViewModel(this.CanModify, value, name)
                    .AddTo(this.CompositeDisposable);
        }

        private void SetupUIConfig(IReadOnlyReactiveProperty<UIConfig> uiConfig)
        {
            // 設定変更時に選択中キャラ別スタイル反映
            Observable
                .CombineLatest(
                    this.VisibleCharaStyles,
                    uiConfig
                        .ObserveInnerProperty(c => c.ExoCharaVoiceroidId)
                        .DistinctUntilChanged(),
                    (vcs, id) => vcs.FirstOrDefault(s => s.VoiceroidId == id) ?? vcs.First())
                .DistinctUntilChanged()
                .Subscribe(s => this.SelectedCharaStyle.Value = s)
                .AddTo(this.CompositeDisposable);

            // 選択中キャラ別スタイル変更時処理
            this.SelectedCharaStyle
                .Where(s => s != null)
                .Subscribe(s => uiConfig.Value.ExoCharaVoiceroidId = s.VoiceroidId)
                .AddTo(this.CompositeDisposable);
            this.SelectedCharaStyle
                .Where(s => s == null)
                .ObserveOnUIDispatcher()
                .Subscribe(
                    _ =>
                        this.SelectedCharaStyle.Value =
                            this.Config.Value.PreviewCharaStyles.First(
                                s => s.VoiceroidId == uiConfig.Value.ExoCharaVoiceroidId))
                .AddTo(this.CompositeDisposable);
        }




        /// <summary>
        /// 選択中タブインデックスを取得する。
        /// </summary>
        public IReactiveProperty<int> SelectedTabIndex { get; }

        /// <summary>
        /// アプリ設定値を取得する。
        /// </summary>
        public IReadOnlyReactiveProperty<AppConfig> Config => this.BaseConfig;

        /// <summary>
        /// 表示状態の YmmCharaRelation コレクションを取得する。
        /// </summary>
        public IReadOnlyReactiveProperty<IReadOnlyCollection<YmmCharaRelation>>
        VisibleYmmCharaRelations
        {
            get;
        }

        /// <summary>
        /// 表示状態の AviUtlDropLayer コレクションを取得する。
        /// </summary>
        public IReadOnlyReactiveProperty<IReadOnlyCollection<AviUtlDropLayer>>
        VisibleAviUtlDropLayers
        {
            get;
        }

        /// <summary>
        /// .exo ファイル作成設定有効化コマンドを表示すべきか否かを取得する。
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsExoFileMakingCommandVisible { get; }

        /// <summary>
        /// .exo ファイル作成設定有効化コマンドを非表示にすべきか否かを取得する。
        /// </summary>
        public IReadOnlyReactiveProperty<bool> IsExoFileMakingCommandInvisible { get; }

        /// <summary>
        /// 保存先ディレクトリ選択コマンドを取得する。
        /// </summary>
        public ICommand SelectSaveDirectoryCommand { get; }

        /// <summary>
        /// 保存先ディレクトリオープンコマンドを取得する。
        /// </summary>
        public ICommand OpenSaveDirectoryCommand { get; }

        /// <summary>
        /// 保存先ディレクトリドラッグオーバーコマンドを取得する。
        /// </summary>
        public ICommand DragOverSaveDirectoryCommand { get; }

        /// <summary>
        /// 保存先ディレクトリドロップコマンドを取得する。
        /// </summary>
        public ICommand DropSaveDirectoryCommand { get; }

        /// <summary>
        /// .exo ファイル作成設定有効化コマンドを取得する。
        /// </summary>
        public ICommand ExoFileMakingCommand { get; }




        /// <summary>
        /// IDataObject オブジェクトから有効なディレクトリパスを検索する。
        /// </summary>
        /// <param name="data">IDataObject オブジェクト。</param>
        /// <returns>ディレクトリパス。見つからなければ null 。</returns>
        private static string FindDirectoryPath(IDataObject data)
        {
            if (data == null)
            {
                return null;
            }

            string path = null;

            if (data.GetDataPresent(DataFormats.FileDrop, true))
            {
                // 複数ディレクトリドロップは不可とする
                var pathes = data.GetData(DataFormats.FileDrop, true) as string[];
                path = (pathes?.Length == 1) ? pathes[0] : null;
            }
            else if (data.GetDataPresent(DataFormats.Text, true))
            {
                path = (data.GetData(DataFormats.Text, true) as string)?.Trim();
                if (path != null)
                {
                    // 相対パスは不可とする
                    if (
                        path.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ||
                        !Path.IsPathRooted(path))
                    {
                        path = null;
                    }
                }
            }

            return (path != null && Directory.Exists(path)) ? path : null;
        }

        /// <summary>
        /// 直近のアプリ状態値の設定先を取得する。
        /// </summary>
        private IReactiveProperty<IAppStatus> LastStatus { get; }

        /// <summary>
        /// ファイル選択ダイアログサービスを取得する。
        /// </summary>
        private IOpenFileDialogService OpenFileDialogService { get; }

        /// <summary>
        /// 保存先ディレクトリパス設定を更新する。
        /// </summary>
        /// <param name="path">ディレクトリパス。</param>
        private void UpdateSaveDirectoryPath(string path)
        {
            // パスが正常かチェック
            var status = FilePathUtil.CheckPathStatus(path, pathIsFile: false);

            // 正常でなければステータス更新のみ
            if (status.StatusType != AppStatusType.None)
            {
                this.LastStatus.Value = status;
                return;
            }

            // フルパスにして設定
            this.Config.Value.SaveDirectoryPath = Path.GetFullPath(path);

            // 成功ステータス設定
            this.SetLastStatus(
                AppStatusType.Success,
                @"保存先フォルダーを設定しました。",
                subStatusText: @"保存先フォルダーを開く",
                subStatusCommand: new ProcessStartCommand(path),
                subStatusCommandTip: path);
        }

        /// <summary>
        /// SelectSaveDirectoryCommand の実処理を行う。
        /// </summary>
        private async Task ExecuteSelectSaveDirectoryCommand()
        {
            // ダイアログ処理
            var path =
                await this.OpenFileDialogService.Run(
                    title: @"音声保存先の選択",
                    initialDirectory: this.Config.Value.SaveDirectoryPath,
                    folderPicker: true);

            // 選択されたなら設定更新
            if (path != null)
            {
                this.UpdateSaveDirectoryPath(path);
            }
        }

        /// <summary>
        /// OpenSaveDirectoryCommand の実処理を行う。
        /// </summary>
        private async Task ExecuteOpenSaveDirectoryCommand()
        {
            var path = this.Config.Value.SaveDirectoryPath;

            if (string.IsNullOrEmpty(path))
            {
                this.SetLastStatus(
                    AppStatusType.Warning,
                    @"保存先フォルダー未設定です。");
                return;
            }
            if (!Directory.Exists(path))
            {
                this.SetLastStatus(
                    AppStatusType.Warning,
                    @"保存先フォルダーが見つかりませんでした。");
                return;
            }

            try
            {
                await Task.Run(() => Process.Start(path));
            }
            catch
            {
                this.SetLastStatus(
                    AppStatusType.Fail,
                    @"保存先フォルダーを開けませんでした。");
                return;
            }
        }

        /// <summary>
        /// DragOverSaveDirectoryCommand の実処理を行う。
        /// </summary>
        /// <param name="e">ドラッグイベントデータ。</param>
        private void ExecuteDragOverSaveDirectoryCommand(DragEventArgs e)
        {
            // 有効なパスがあれば受け入れエフェクト設定
            if (FindDirectoryPath(e?.Data) != null)
            {
                e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                e.Handled = true;
            }
        }

        /// <summary>
        /// DropSaveDirectoryCommand の実処理を行う。
        /// </summary>
        /// <param name="e">ドラッグイベントデータ。</param>
        private void ExecuteDropSaveDirectoryCommand(DragEventArgs e)
        {
            var path = FindDirectoryPath(e?.Data);
            if (path == null)
            {
                return;
            }

            e.Handled = true;

            // 末尾がディレクトリ区切り文字なら削除
            if (Path.GetFileName(path).Length == 0)
            {
                // ドライブルートの場合は null が返ってくる
                path = Path.GetDirectoryName(path) ?? path;
            }

            // 設定更新
            this.UpdateSaveDirectoryPath(path);
        }

        /// <summary>
        /// ExoFileMakingCommand の実処理を行う。
        /// </summary>
        private void ExecuteExoFileMakingCommand()
        {
            if (
                !this.CanModify.Value ||
                this.Config.Value?.IsExoFileMaking != false)
            {
                return;
            }

            this.Config.Value.IsExoFileMaking = true;

            this.LastStatus.Value =
                new AppStatus
                {
                    StatusType = AppStatusType.Success,
                    StatusText = @".exo ファイル作成設定を有効にしました。",
                };
        }

        /// <summary>
        /// 直近のアプリ状態を設定する。
        /// </summary>
        /// <param name="statusType">状態種別。</param>
        /// <param name="statusText">状態テキスト。</param>
        /// <param name="subStatusType">オプショナルなサブ状態種別。</param>
        /// <param name="subStatusText">オプショナルなサブ状態テキスト。</param>
        /// <param name="subStatusCommand">オプショナルなサブ状態コマンド。</param>
        /// <param name="subStatusCommandTip">
        /// オプショナルなサブ状態コマンドのチップテキスト。
        /// </param>
        private void SetLastStatus(
            AppStatusType statusType = AppStatusType.None,
            string statusText = "",
            AppStatusType subStatusType = AppStatusType.None,
            string subStatusText = "",
            ICommand subStatusCommand = null,
            string subStatusCommandTip = "")
            =>
            this.LastStatus.Value =
                new AppStatus
                {
                    StatusType = statusType,
                    StatusText = statusText ?? "",
                    SubStatusType = subStatusType,
                    SubStatusText = subStatusText ?? "",
                    SubStatusCommand = subStatusCommand,
                    SubStatusCommandTip =
                        string.IsNullOrEmpty(subStatusCommandTip) ?
                            null : subStatusCommandTip,
                };




        #region デザイン時用定義

        /// <summary>
        /// デザイン時用コンストラクタ。
        /// </summary>
        [Obsolete(@"Design time only.")]
        public AppConfigViewModel()
            :
            this(
                new ReactiveProperty<bool>(true),
                new ReactiveProperty<AppConfig>(new AppConfig { IsExoFileMaking = true }),
                new ReactiveProperty<UIConfig>(new UIConfig()),
                new ReactiveProperty<IAppStatus>(new AppStatus()),
                NullServices.OpenFileDialog)
        {
        }

        #endregion
    }
}
