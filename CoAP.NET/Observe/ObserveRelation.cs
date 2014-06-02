﻿/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using CoAP.Log;
using CoAP.Net;
using CoAP.Server.Resources;

namespace CoAP.Observe
{
    /// <summary>
    /// Represents a relation between a client endpoint and a resource on this server.
    /// </summary>
    public class ObserveRelation
    {
        static readonly ILogger log = LogManager.GetLogger(typeof(ObserveRelation));
        readonly ObserveNotificationOrderer _orderer = new ObserveNotificationOrderer();
        readonly ObservingEndpoint _endpoint;
        readonly IResource _resource;
        readonly Exchange _exchange;
        readonly Int64 _checkIntervalTime = 24 * 60 * 60 * 1000;
        readonly Int32 _checkIntervalCount = 100;
        private Response _recentControlNotification;
        private Response _nextControlNotification;
        private Boolean _established;
        private DateTime _interestCheckTime = DateTime.Now;
        private Int32 _interestCheckCounter = 1;

        /// <summary>
        /// Constructs a new observe relation.
        /// </summary>
        /// <param name="endpoint">the observing endpoint</param>
        /// <param name="resource">the observed resource</param>
        /// <param name="exchange">the exchange that tries to establish the observe relation</param>
        public ObserveRelation(ObservingEndpoint endpoint, IResource resource, Exchange exchange)
        {
            if (endpoint == null)
                throw new ArgumentNullException("endpoint");
            if (resource == null)
                throw new ArgumentNullException("resource");
            if (exchange == null)
                throw new ArgumentNullException("exchange");
            _endpoint = endpoint;
            _resource = resource;
            _exchange = exchange;
        }

        /// <summary>
        /// Gets the resource.
        /// </summary>
        public IResource Resource
        {
            get { return _resource; }
        }

        /// <summary>
        /// Gets the exchange.
        /// </summary>
        public Exchange Exchange
        {
            get { return _exchange; }
        }

        /// <summary>
        /// Gets the source endpoint of the observing endpoint.
        /// </summary>
        public System.Net.EndPoint Source
        {
            get { return _endpoint.EndPoint; }
        }

        public Response CurrentControlNotification
        {
            get { return _recentControlNotification; }
            set { _recentControlNotification = value; }
        }

        public Response NextControlNotification
        {
            get { return _nextControlNotification; }
            set { _nextControlNotification = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if this relation has been established.
        /// </summary>
        public Boolean Established
        {
            get { return _established; }
            set { _established = value; }
        }

        /// <summary>
        /// Cancel this observe relation.
        /// </summary>
        public void Cancel()
        {
            if (log.IsDebugEnabled)
                log.Debug("Cancel observe relation from " + _endpoint.EndPoint + " with " + _resource.Path);
            _established = false;
            _resource.RemoveObserveRelation(this);
            _endpoint.RemoveObserveRelation(this);
        }

        /// <summary>
        /// Cancel all observer relations that this server has
        /// established with this's realtion's endpoint.
        /// </summary>
        public void CancelAll()
        {
            _endpoint.CancelAll();
        }

        /// <summary>
        /// Notifies the observing endpoint that the resource has been changed.
        /// </summary>
        public void NotifyObservers()
        {
            // makes the resource process the same request again
            _resource.HandleRequest(_exchange);
        }

        public Boolean Check()
        {
            Boolean check = false;
            DateTime now = DateTime.Now;
            check |= _interestCheckTime.AddMilliseconds(_checkIntervalTime) < now;
            check |= (++_interestCheckCounter >= _checkIntervalCount);
            if (check)
            {
                _interestCheckTime = now;
                _interestCheckCounter = 0;
            }
            return check;
        }
    }
}