import {Component, Event, EventEmitter, h, Listen, Method, Prop} from '@stencil/core';
import Tunnel, {DashboardState} from "../../../../data/dashboard";
import {ElsaStudio, WorkflowModel} from "../../../../models";
import {
  eventBus,
  pluginManager,
  activityIconProvider,
  confirmDialogService,
  toastNotificationService,
  createElsaClient,
  createHttpClient,
  ElsaClient,
  propertyDisplayManager
} from "../../../../services";
import {AxiosInstance} from "axios";
import {EventTypes} from "../../../../models";
import {ToastNotificationOptions} from "../../../shared/elsa-toast-notification/elsa-toast-notification";
import {getOrCreateProperty, htmlToElement} from "../../../../utils/utils";

@Component({
  tag: 'elsa-studio-root',
  shadow: false
})
export class ElsaStudioRoot {

  @Prop({attribute: 'server-url', reflect: true}) serverUrl: string;
  @Prop({attribute: 'monaco-lib-path', reflect: true}) monacoLibPath: string;
  @Prop({attribute: 'culture', reflect: true}) culture: string;
  @Prop({attribute: 'base-path', reflect: true}) basePath: string = '';
  @Prop({attribute: 'features', reflect: true}) featuresString: string;
  @Event() initializing: EventEmitter<ElsaStudio>;
  @Event() initialized: EventEmitter<ElsaStudio>;

  private confirmDialog: HTMLElsaConfirmDialogElement;
  private toastNotificationElement: HTMLElsaToastNotificationElement;
  private elsaStudio: ElsaStudio;

  @Method()
  async addPlugins(pluginTypes: Array<any>) {
    pluginManager.registerPlugins(pluginTypes);
  }

  @Method()
  async addPlugin(pluginType: any) {
    pluginManager.registerPlugin(pluginType);
  }

  @Listen('workflow-changed')
  workflowChangedHandler(event: CustomEvent<WorkflowModel>) {
    eventBus.emit(EventTypes.WorkflowModelChanged, this, event.detail);
  }

  connectedCallback() {
    eventBus.on(EventTypes.ShowConfirmDialog, this.onShowConfirmDialog);
    eventBus.on(EventTypes.HideConfirmDialog, this.onHideConfirmDialog);
    eventBus.on(EventTypes.ShowToastNotification, this.onShowToastNotification);
    eventBus.on(EventTypes.HideToastNotification, this.onHideToastNotification);
  }

  disconnectedCallback() {
    eventBus.detach(EventTypes.ShowConfirmDialog, this.onShowConfirmDialog);
    eventBus.detach(EventTypes.HideConfirmDialog, this.onHideConfirmDialog);
    eventBus.detach(EventTypes.ShowToastNotification, this.onShowToastNotification);
    eventBus.detach(EventTypes.HideToastNotification, this.onHideToastNotification);
  }

  async componentWillLoad() {
    const elsaClientFactory: () => Promise<ElsaClient> = () => createElsaClient(this.serverUrl);
    const httpClientFactory: () => Promise<AxiosInstance> = () => createHttpClient(this.serverUrl);

    const elsaClient = await elsaClientFactory();

    const elsaStudio: ElsaStudio = this.elsaStudio = {
      serverUrl: this.serverUrl,
      basePath: this.basePath,
      featuresString: this.featuresString,
      serverFeatures: [],
      eventBus,
      pluginManager,
      propertyDisplayManager,
      activityIconProvider,
      confirmDialogService,
      toastNotificationService,
      elsaClientFactory,
      httpClientFactory,
      getOrCreateProperty: getOrCreateProperty,
      htmlToElement
    };

    this.initializing.emit(elsaStudio);
    await eventBus.emit(EventTypes.Root.Initializing);
    pluginManager.initialize(elsaStudio);
    propertyDisplayManager.initialize(elsaStudio);

    elsaStudio.serverFeatures = await elsaClient.featuresApi.list();
  }

  async componentDidLoad() {
    this.initialized.emit(this.elsaStudio);
    await eventBus.emit(EventTypes.Root.Initialized);
  }

  onShowConfirmDialog = (e) => e.promise = this.confirmDialog.show(e.caption, e.message)
  onHideConfirmDialog = async () => await this.confirmDialog.hide()
  onShowToastNotification = async (e: ToastNotificationOptions) => await this.toastNotificationElement.show(e)
  onHideToastNotification = async () => await this.toastNotificationElement.hide()

  render() {

    const culture = this.culture;

    const tunnelState: DashboardState = {
      serverUrl: this.serverUrl,
      basePath: this.basePath,
      serverFeatures: this.elsaStudio.serverFeatures,
      culture,
      monacoLibPath: this.monacoLibPath
    };

    return (
      <Tunnel.Provider state={tunnelState}>
        <slot/>
        <elsa-confirm-dialog ref={el => this.confirmDialog = el} culture={this.culture}/>
        <elsa-toast-notification ref={el => this.toastNotificationElement = el}/>
      </Tunnel.Provider>
    );
  }
}
