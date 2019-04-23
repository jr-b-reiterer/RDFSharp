﻿/*
   Copyright 2012-2019 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAvgAggregator represents an AVG aggregation function applied by a GroupBy modifier
    /// </summary>
    public class RDFAvgAggregator: RDFAggregator
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build an AVG aggregator on the given variable and with the given projection name
        /// </summary>
        public RDFAvgAggregator(RDFVariable aggrVariable, RDFVariable projVariable) : base(aggrVariable, projVariable) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the AVG aggregator
        /// </summary>
        public override String ToString()
        {
            return String.Format("(AVG({0}) AS {1})", this.AggregatorVariable, this.ProjectionVariable);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the AVG aggregator function on the given tablerow
        /// </summary>
        internal override void ExecuteAggregatorFunction(Dictionary<String, Dictionary<String, Object>> partitionRegistry, String partitionKey, DataRow tableRow)
        {
            //Get the row value
            Decimal rowValue = GetRowValueAsDecimal(tableRow);
            //Get the aggregator value
            Decimal aggregatorValue = GetAggregatorValueAsDecimal(partitionRegistry, partitionKey);
            //Update the aggregator value
            SetAggregatorValue(partitionRegistry, partitionKey, rowValue + aggregatorValue);
            //Update the table metadata
            UpdateAggregatorContext(partitionRegistry, partitionKey, tableRow);
        }

        /// <summary>
        /// Updates the context of the aggregator after current execution
        /// </summary>
        internal void UpdateAggregatorContext(Dictionary<String, Dictionary<String, Object>> partitionRegistry, String partitionKey, DataRow tableRow)
        {
            String extendedPropertyKey = partitionKey + "§§" + this.ProjectionVariable.VariableName;
            if (!tableRow.Table.ExtendedProperties.ContainsKey(extendedPropertyKey))
            {
                tableRow.Table.ExtendedProperties.Add(extendedPropertyKey, Decimal.One);
            }
            else
            {
                tableRow.Table.ExtendedProperties[extendedPropertyKey] = ((Decimal)tableRow.Table.ExtendedProperties[extendedPropertyKey]) + 1;
            }
        }

        /// <summary>
        /// Finalizes the AVG aggregator function on the result table
        /// </summary>
        internal override void FinalizeAggregatorFunction(Dictionary<String, Dictionary<String, Object>> partitionRegistry, DataTable workingTable)
        {
            foreach(String partitionKey in partitionRegistry.Keys)
            {
                //Get the aggregator value
                Decimal aggregatorValue = GetAggregatorValueAsDecimal(partitionRegistry, partitionKey);
                //Get the aggregator context
                String extendedPropertyKey = partitionKey + "§§" + this.ProjectionVariable.VariableName;
                Decimal aggregatorContext = (Decimal)workingTable.ExtendedProperties[extendedPropertyKey];
                //Update the aggregator value
                SetAggregatorValue(partitionRegistry, partitionKey, aggregatorValue / aggregatorContext);
            }
        }
        #endregion

    }

}