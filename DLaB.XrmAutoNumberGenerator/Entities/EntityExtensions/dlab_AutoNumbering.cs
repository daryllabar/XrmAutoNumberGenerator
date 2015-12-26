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


        public int BatchStep => dlab_ServerBatchSize.GetValueOrDefault(1) * dlab_IncrementStepSize.GetValueOrDefault(1);

        /// <summary>
        /// Queues the generated values in the Batch using the settings.  Returns the most current Setting Value
        /// </summary>
        /// <returns></returns>
        public static dlab_AutoNumbering EnqueueNextBatch(IOrganizationService service, dlab_AutoNumbering setting, Queue<string> queue)
        {
            int currentNumber;
            if (string.IsNullOrWhiteSpace(setting.RowVersion))
            {
                // ***** Fixed in 7.1.1.4309 *****
                // CRM 7.1.1.4210 currently has a bugg where the row version is not being returned from within a plugin.
                // This should get fixed in the near future.  This will serve as a stop gap until then
                //setting = service.GetEntity<dlab_AutoNumbering>(setting.Id);
                //currentNumber = setting.IncrementNextNumber();
                //service.Update(setting.CreateUpdateNextNumberEntity());
                //setting.EnqueueBatchValues(queue, currentNumber);
                //return setting;
                throw new InvalidPluginExecutionException("Row Version returned from the server was null!  Unable to guarantee Uniqueness!  This was a bug in 7.1.1.4210 that was fixed in 7.1.1.4309");
            }

            currentNumber = setting.IncrementNextNumber();
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
                dlab_NextNumber = dlab_NextNumber
            };
        }

        private string GenerateFormattedNumber(int currentNumber)
        {
            var number = (dlab_PadWithZeros.GetValueOrDefault(false)
                ? currentNumber.ToString().PadLeft(dlab_FixedNumberSize.GetValueOrDefault(1), '0')
                : currentNumber.ToString());
            return
                $"{dlab_Prefix}{number}{dlab_Postfix}";
        }

        /// <summary>
        /// Determine if the auto-number value is already populated and the setting says to not override it
        /// </summary>
        /// <param name="entity">The entity to lookup the auto-gened attribute for.</param>
        /// <returns></returns>
        public bool UseInitializedValue(Entity entity)
        {
            return !dlab_AllowExternalInitialization.GetValueOrDefault() &&
                   entity.Contains(AttributeName) &&
                   string.IsNullOrWhiteSpace((entity[AttributeName] ?? string.Empty).ToString());
        }
    }
}
