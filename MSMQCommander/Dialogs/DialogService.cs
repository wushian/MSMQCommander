﻿using System.Messaging;
using System.Windows;
using Caliburn.Micro;
using MSMQCommander.Contex;
using MSMQCommander.ViewModels.Dialogs;
using MSMQCommander.Views.Dialogs;
using Microsoft.Win32;
using MsmqLib;

namespace MSMQCommander.Dialogs
{
    public interface IDialogService
    {
        MessageBoxResult AskQuestion(string question, string caption, MessageBoxButton button);
        void ConnectToComputer();
        void ShowError(string errorMessageFormat, params object[] args);
        void ExportMessageBody(MessageQueue messageQueue, string messageId);
        bool ImportMessageBody(MessageQueue messageQueue);
        bool CreateNewMessage(MessageQueue messageQueue);
        bool DeleteMessage(MessageQueue messageQueue, string messageId);
        bool CreateNewQueue();
        bool DeleteQueue(MessageQueue messageQueue);

        bool ExportAllMessagesToQueue(string sourceQueue, string messageId = null);
    }

    public class DialogService : IDialogService
    {
        private readonly IWindowManager _windowManager;
        private readonly QueueConnectionContext _queueConnectionContext;
        private readonly IQueueService _queueService;

        public DialogService(IWindowManager windowManager, QueueConnectionContext queueConnectionContext, IQueueService queueService)
        {
            _windowManager = windowManager;
            _queueConnectionContext = queueConnectionContext;
            _queueService = queueService;
        }

        public MessageBoxResult AskQuestion(string question, string caption, MessageBoxButton button)
        {
            return MessageBox.Show(question, caption, button, MessageBoxImage.Question);
        }

        public void ConnectToComputer()
        {
            var viewModel = (ConfigureConnectionViewModel)ViewModelLocator.LocateForViewType(typeof (ConfigureConnectionView));
            if (_windowManager.ShowDialog(viewModel) == true)
            {
                _queueConnectionContext.UpdateComputerName(viewModel.ComputerName);
            }
        }

        public void ShowError(string errorMessageFormat, params object[] args)
        {
            var message = string.Format(errorMessageFormat, args);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ExportMessageBody(MessageQueue messageQueue, string messageId)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                string errorMessage;
                if (false == _queueService.ExportMessageBody(messageQueue, messageId, filePath, out errorMessage))
                {
                    ShowError("Failed to export to file '{0}': {1}", filePath, errorMessage);
                }
            }
        }

        public bool ImportMessageBody(MessageQueue messageQueue)
        {
            var viewModel = (ImportMessageBodyViewModel)ViewModelLocator.LocateForViewType(typeof(ImportMessageBodyView));
            viewModel.Initialize(messageQueue);
            return _windowManager.ShowDialog(viewModel) == true;
        }

        public bool CreateNewMessage(MessageQueue messageQueue)
        {
            var viewModel = (CreateNewMessageViewModel)ViewModelLocator.LocateForViewType(typeof(CreateNewMessageView));
            viewModel.Initialize(messageQueue);
            return _windowManager.ShowDialog(viewModel).GetValueOrDefault();
        }

        public bool DeleteMessage(MessageQueue messageQueue, string messageId)
        {
            var question = string.Format("Delete message id {0}?", messageId);
            if (AskQuestion(question, "Delete message", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                string errorMessage;
                if(false == _queueService.DeleteMessage(messageQueue, messageId, out errorMessage))
                {
                    ShowError("Failed to delete message id {0}: {1}", messageId, errorMessage);
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool CreateNewQueue()
        {
            var viewModel = (CreateNewQueueViewModel) ViewModelLocator.LocateForViewType(typeof (CreateNewQueueView));
            return _windowManager.ShowDialog(viewModel).GetValueOrDefault();
        }

        public bool ExportAllMessagesToQueue(string sourceQueue, string messageId = null) {
            var viewModel = (ExportAllMessagesToQueueViewModel)ViewModelLocator.LocateForViewType(typeof(ExportAllMessagesToQueueView));
            viewModel.Initialize(sourceQueue, messageId);
            return _windowManager.ShowDialog(viewModel).GetValueOrDefault();
        }

        public bool DeleteQueue(MessageQueue messageQueue)
        {
            var question = string.Format("Delete the queue '{0}'?", messageQueue.QueueName);
            if (AskQuestion(question, "Delete queue", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                string errorMessage;
                if (false == _queueService.DeleteQueue(messageQueue, out errorMessage))
                {
                    ShowError("Failed to delete the message queue '{0}': {1}", messageQueue.QueueName, errorMessage);
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}