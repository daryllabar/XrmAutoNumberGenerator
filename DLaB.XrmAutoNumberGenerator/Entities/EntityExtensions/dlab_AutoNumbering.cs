using System.Collections.Generic;
using DLaB.Xrm;
using Microsoft.Xrm.Sdk;

// ReSharper disable once CheckNamespace
namespace DLaB.XrmAutoNumberGenerator.Entities
{
    // ReSharper disable once InconsistentNaming
    partial class dlab_AutoNumbering
    {
        /// <summary>
        /// Lower Cases the dlab_AttributeName
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public string AttributeName => dlab_AttributeName.ToLower();


        /// <summary>
        /// Lower Cases the dlab_EntityName
        /// </summary>
        /// <value>
        /// The name of the entity.
        /// </value>
        public string EntityName => dlab_EntityName.ToLower();

        /// <summary>
        /// Returns EntityName.AttributeName
        /// </summary>
        public string FullName => $"{EntityName}.{AttributeName}";

        public int BatchStep => dlab_ServerBatchSize.GetValueOrDefault(1) * dlab_IncrementStepSize.GetValueOrDefault(1);

        /// <summary>
        /// Queues the generated values in the Batch using the settings.  Returns the most current Setting Value
        /// </summary>
        /// <returns></returns>
        public static dlab_AutoNumbering EnqueueNextBatch(IOrganizationService service, dlab_AutoNumbering setting, Queue<string> queue, ITracingService log)
        {
            int currentNumber;
            if (string.IsNullOrWhiteSpace(setting.RowVersion))
            {
                // Some older versions of CRM don't contain RowVersions
                log.Trace("No Row Version found.  Performing Non-Thread Safe Update.");
                setting = service.GetEntity<dlab_AutoNumbering>(setting.Id);
                currentNumber = setting.IncrementNextNumber();
                log.Trace("Grabbing values {0} to {1} and updating setting.", currentNumber, setting.dlab_NextNumber - 1);
                service.Update(setting.CreateUpdateNextNumberEntity());
                setting.EnqueueBatchValues(queue, currentNumber);
                return setting;
            }

            log.Trace("Row Version found.  Performing Thread Safe Update.");
            currentNumber = setting.IncrementNextNumber();
            log.Trace("Grabbing values {0} to {1} and updating setting.", currentNumber, setting.dlab_NextNumber - 1);
            service.OptimisticUpdate(
                setting.CreateUpdateNextNumberEntity(),
                s =>
                {
                    setting = s;
                    currentNumber = s.IncrementNextNumber();
                    return s.CreateUpdateNextNumberEntity();
                });

            setting.EnqueueBatchValues(queue, currentNumber);
            return setting;
        }

        private void EnqueueBatchValues(Queue<string> queue, int currentNumber)
        {

            for (var i = currentNumber; i < dlab_NextNumber; i += dlab_IncrementStepSize.GetValueOrDefault(1))
            {
                queue.Enqueue(GenerateFormattedNumber(i));
            }
        }

        /// <summary>
        /// Increments the next number by the Batch Step size, returning the current number;
        /// </summary>
        /// <returns></returns>
        private int IncrementNextNumber()
        {
            var currentNumber = dlab_NextNumber.GetValueOrDefault(0);
            dlab_NextNumber = currentNumber + BatchStep;
            return currentNumber;
        }

        private dlab_AutoNumbering CreateUpdateNextNumberEntity()
        {
            return new dlab_AutoNumbering
            {
                Id = Id,
                dlab_NextNumber = dlab_NextNumber,
                RowVersion = RowVersion
            };
        }

        private string GenerateFormattedNumber(int currentNumber)
        {
            return 
                dlab_Prefix +
                (
                    dlab_PadWithZeros.GetValueOrDefault(false) 
                        ? currentNumber.ToString().PadLeft(dlab_FixedNumberSize.GetValueOrDefault(1), '0') 
                        : currentNumber.ToString()
                ) +
                dlab_Postfix;
        }

        /// <summary>
        /// Determine if the auto-number value is already populated and the setting says to not override it
        /// </summary>
        /// <param name="entity">The entity to lookup the auto-gened attribute for.</param>
        /// <returns></returns>
        public bool UseInitializedValue(Entity entity)
        {
            return dlab_AllowExternalInitialization.GetValueOrDefault() &&
                   entity.Contains(AttributeName) &&
                   !string.IsNullOrWhiteSpace(entity.GetAttributeValue<string>(AttributeName));
        }
    }
}
