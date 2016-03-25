// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCrudActorWithNotifications.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic actor to perform basic crud operation on EF objects and sends notification of successfull operations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Data
{
    using System;
    using Akka.Actor;
    using ClusterKit.Core.Rest.ActionMessages;
    using JetBrains.Annotations;

    /// <summary>
    /// Generic actor to perform basic crud operation on EF objects and sends <seealso cref="UpdateMessage{TObject}"/> of successful operations
    /// </summary>
    /// <typeparam name="TContext">
    /// The database context
    /// </typeparam>
    public abstract class BaseCrudActorWithNotifications<TContext> : BaseCrudActor<TContext>
        where TContext : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCrudActorWithNotifications{TContext}"/> class.
        /// </summary>
        /// <param name="notificationReceiver">
        /// Reference to actor to receive notifications
        /// </param>
        protected BaseCrudActorWithNotifications(IActorRef notificationReceiver)
        {
            this.NotificationReceiver = notificationReceiver;
        }

        /// <summary>
        /// Gets the reference to actor to receive notifications
        /// </summary>
        [UsedImplicitly]
        protected IActorRef NotificationReceiver { get; }

        /// <summary>
        /// Method called after successful object creation in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="result">Created object</param>
        protected override void AfterCreate<TObject>(TObject result)
        {
            this.NotificationReceiver.Tell(
                           new UpdateMessage<TObject> { ActionType = EnActionType.Create, NewObject = result });
            base.AfterCreate(result);
        }

        /// <summary>
        /// Method called after successful object removal from database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="deletedObject">The removed object</param>
        protected override void AfterDelete<TObject>(TObject deletedObject)
        {
            this.NotificationReceiver.Tell(
                    new UpdateMessage<TObject> { ActionType = EnActionType.Delete, OldObject = deletedObject });
            base.AfterDelete(deletedObject);
        }

        /// <summary>
        /// Method called after successful object modification in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="newObject">
        /// The new Object.
        /// </param>
        /// <param name="oldObject">
        /// The old Object.
        /// </param>
        protected override void AfterUpdate<TObject>(TObject newObject, TObject oldObject)
        {
            this.NotificationReceiver.Tell(
                                new UpdateMessage<TObject>
                                {
                                    ActionType = EnActionType.Update,
                                    NewObject = newObject,
                                    OldObject = oldObject
                                });
            base.AfterUpdate(newObject, oldObject);
        }
    }
}