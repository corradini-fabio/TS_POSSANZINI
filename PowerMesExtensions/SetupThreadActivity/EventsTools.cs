using Atys.PowerMES.Events;
using Atys.PowerMES.Foundation;
using Atys.PowerMES.Support;
using System;
using System.Globalization;

namespace TeamSystem.Customizations.SetupThreadActivity
{
    internal class EventsTools
    {
        private string _Article = string.Empty;
        private string _Wo = string.Empty;
        private string _Phase = string.Empty;
        private string _Resourcename = string.Empty;
        private string _EventData = string.Empty;
        private string _StartData = string.Empty;
        private string _EndData = string.Empty;
        private int _Qty = 0;
        private int _RejectedQty = 0;
        private IMesManager _MesManager = null;
        private IMesAppLogger _MesAppLogger = null;
        private ArticleItem _ArticleItem = null;

        public EventsTools()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName">Nome risorsa PowerMes</param>
        /// <param name="article">Codice Articolo</param>
        /// <param name="wo">Numero Bolla</param>
        /// <param name="eventData">Data Evento</param>
        /// <param name="mesManager">Riferimento a _MesManager</param>
        /// <param name="mesAppLogger">riferimento a Log applicazione</param>
        public EventsTools(string resourceName, string article, string wo, string eventData, IMesManager mesManager)
        {
            this._Resourcename = resourceName;
            this._Article = article;
            this._Wo = wo;
            this._EventData = eventData;
            this._MesManager = mesManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName">Nome risorsa PowerMes</param>
        /// <param name="articleItem">ArticleItem</param>
        /// <param name="wo">Numero Bolla</param>
        /// <param name="eventData">Data Evento</param>
        /// <param name="mesManager">Riferimento a _MesManager</param>
        /// <param name="mesAppLogger">riferimento a Log applicazione</param>
        public EventsTools(string resourceName, ArticleItem articleItem, string wo, string eventData, int qty, IMesManager mesManager)
        {
            this._Resourcename = resourceName;
            this._ArticleItem = articleItem;
            this._Wo = wo;
            this._EventData = eventData;
            this._Qty= qty;
            this._MesManager = mesManager;
        }

        /// <summary>
        /// Oggetto per invio eventi
        /// </summary>
        /// <param name="resourceName">nome risorsa MES</param>
        /// <param name="article">codice articolo</param>
        /// <param name="wo">bolla</param>
        /// <param name="startData">data inizio evento</param>
        /// <param name="endData">data fine evento</param>
        /// <param name="qty">pezzi buoni</param>
        /// <param name="mesManager">riferimento a MesManager</param>
        public EventsTools(string resourceName, ArticleItem articleItem, string wo, string startData, string endData, int qty, IMesManager mesManager)
        {
            this._Resourcename = resourceName;
            this._ArticleItem = articleItem;
            this._Wo = wo;
            this._StartData = startData;
            this._EndData = endData;
            this._Qty = qty;
            this._MesManager = mesManager;
        }

        /// <summary>
        /// Oggetto per invio eventi
        /// </summary>
        /// <param name="resourceName">nome risorsa MES</param>
        /// <param name="article">codice articolo</param>
        /// <param name="wo">bolla</param>
        /// <param name="startData">data inizio evento</param>
        /// <param name="endData">data fine evento</param>
        /// <param name="qty">pezzi buoni</param>
        /// <param name="rejectedQty">pezzi scarto</param>
        /// <param name="mesManager">riferimento a MesManager</param>
        public EventsTools(string resourceName, string article, string wo, string startData, string endData, int qty, int rejectedQty, IMesManager mesManager)
        {
            this._Resourcename = resourceName;
            this._Article = article;
            this._Wo = wo;
            this._StartData = startData;
            this._EndData = endData;
            this._Qty = qty;
            this._RejectedQty = qty;
            this._MesManager = mesManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName">Nome risorsa PowerMes</param>
        /// <param name="article">Codice Articolo</param>
        /// <param name="phase">Codice Fase</param>
        /// <param name="wo">Numero Bolla</param>
        /// <param name="startData">Data inizio Evento</param>
        /// <param name="endData">Data fine evento</param>
        /// <param name="qty">Quantità da versare</param>
        /// <param name="mesManager">Riferimento a _MesManager</param>
        /// <param name="mesAppLogger">riferimento a Log applicazione</param>
        public EventsTools(string resourceName, string article, string phase, string wo, string startData, string endData, int qty, IMesManager mesManager, IMesAppLogger mesAppLogger)
        {
            this._Resourcename = resourceName;
            this._Article = article;
            this._ArticleItem = new ArticleItem(_Article, "10");
            this._Phase = phase;
            this._Wo = wo;
            this._StartData = startData;
            this._EventData = startData; //Lo uso per evento start già usato in altro punto del codice
            this._EndData = endData;
            this._Qty = qty;
            this._MesManager = mesManager;
            this._MesAppLogger = mesAppLogger;
        }

        /// <summary>
        /// Invia un evento di start senza specificare articolo, fase e bolla.
        /// </summary>
        /// <param name="timeout"></param>
        public void SendStartWithWo(int timeout = 0)
        {
            //var articleItem = new ArticleItem(_Article, "10");
            var mesEvent = new ArticleStartedEvent(this._Resourcename,
                                  DateTime.ParseExact(_StartData, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToUniversalTime(),
                                                      _ArticleItem,
                                                      timeout,
                                                      "",
                                                      0,
                                                      "",
                                                      100,
                                                      _Wo);

            bool enqueueEvent = this._MesManager.DataInputFunnel.EnqueueEvent(mesEvent);
            this._MesManager.AppendMessageToLog(MessageLevel.Info,
                                                nameof(SendStartWithWo),
                                                $"SendStartNoData = {enqueueEvent} per la macchina {this._Resourcename}");
        }

        /// <summary>
        /// Invia comando ProductDone con QTA=1
        /// </summary>
        public void SendDone()
        {
            try
            {
                //var articleItem = new ArticleItem(this._Article, "10");
                var mesEvent = new ProductDoneEvent(this._Resourcename,
                                   DateTime.ParseExact(_EndData, "MM/dd/yyyy HH:mm:ss", CultureInfo.InstalledUICulture).ToUniversalTime(),
                                                       _ArticleItem,
                                                       _Qty,
                                                       _RejectedQty,
                                                       0,
                                                       this._Wo.ToString(),
                                                       0);

                bool enqueueEvent = this._MesManager.DataInputFunnel.EnqueueEvent(mesEvent);
                this._MesManager.AppendMessageToLog(MessageLevel.Info,
                                                    nameof(SendDone),
                                                    $"SendDoneNoData = {enqueueEvent} per la macchina {this._Resourcename}");
            }
            catch (Exception ex)
            {

                this._MesManager.AppendMessageToLog(MessageLevel.Error,
                                                nameof(SendDone),
                                                ex.ToString()
                                                );
            }
        }

        /// <summary>
        /// Invia comando ProductDone con QTA=1
        /// </summary>
        public void SendDone2()
        {
            try
            {
                _ArticleItem = new ArticleItem(this._Article, _Phase);
                var mesEvent = new ProductDoneEvent(this._Resourcename,
                                   DateTime.ParseExact(_EndData, "MM/dd/yyyy HH:mm:ss", CultureInfo.InstalledUICulture).ToUniversalTime(),
                                                       _ArticleItem,
                                                       _Qty,
                                                       _RejectedQty,
                                                       0,
                                                       this._Wo.ToString(),
                                                       0);

                bool enqueueEvent = this._MesManager.DataInputFunnel.EnqueueEvent(mesEvent);
                this._MesManager.AppendMessageToLog(MessageLevel.Info,
                                                    nameof(SendDone),
                                                    $"SendDoneNoData = {enqueueEvent} per la macchina {this._Resourcename}");
            }
            catch (Exception ex)
            {

                this._MesManager.AppendMessageToLog(MessageLevel.Error,
                                                nameof(SendDone),
                                                ex.ToString()
                                                );
            }
        }

        /// <summary>
        /// Invia sospensione generica con causale 1=S-SOG
        /// </summary>
        public void SendGenericSuspension()
        {
            Guid correlationId = new Guid();
            var mesEvent = new GenericSuspensionEvent(correlationId,
                                                      this._Resourcename,
                                                      DateTime.ParseExact(this._EventData, "MM/dd/yyyy HH:mm:ss", CultureInfo.InstalledUICulture).ToUniversalTime(),
                                                      "",
                                                      "1",
                                                      0
                                                      );

            bool enqueueEvent = this._MesManager.DataInputFunnel.EnqueueEvent(mesEvent);
            this._MesManager.AppendMessageToLog(MessageLevel.Info,
                                                nameof(SendGenericSuspension),
                                                $"SendGenericSuspension = {enqueueEvent} per la macchina {this._Resourcename}");
        }
    }
}
