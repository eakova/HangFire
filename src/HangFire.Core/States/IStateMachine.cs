// This file is part of HangFire.
// Copyright � 2013-2014 Sergey Odinokov.
// 
// HangFire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// HangFire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with HangFire. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using HangFire.Common;
using HangFire.Common.States;

namespace HangFire.States
{
    public interface IStateMachine
    {
        string CreateInState(Job job, IDictionary<string, string> parameters, State state);
        bool TryToChangeState(string jobId, State toState, string[] fromStates);
    }
}