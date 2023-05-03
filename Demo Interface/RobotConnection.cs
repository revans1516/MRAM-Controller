using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.Robotics.Controllers;

namespace Demo_Interface
{
	public class RobotConnection
	{
		public enum Robot_Status
		{
			Ready = 1,
			Disconnected = -1,
			Manual_Mode = -2,
			Motors_Off = -3,
			Running = -4
		}


		private Controller _Controller;
		public Controller controller
		{
			get { return _Controller; }
		}



        private System.Net.IPAddress _IPAddress;
		public System.Net.IPAddress IPAddress
		{
			get { return _IPAddress; }

		}


		private Robot_Status _RobState;
		public Robot_Status RobotState
		{
			get { return _RobState; } 
		}


		private event EventHandler RobStateChg;


		


		RobotConnection (System.Net.IPAddress RobIPadd )
		{
			_IPAddress = RobIPadd;
			ConnectController();
			_Controller.StateChanged += Robot_Check;
			_Controller.ConnectionChanged += Robot_Check;
			_Controller.Rapid.ExecutionStatusChanged+= Robot_Check; 
			_Controller.OperatingModeChanged+=Robot_Check;
		}


		public Boolean ConnectController()
		{
            ABB.Robotics.Controllers.Discovery.NetworkScanner scanner= new ABB.Robotics.Controllers.Discovery.NetworkScanner();
            ABB.Robotics.Controllers.ControllerInfoCollection controllerInfos;

            
            if (_Controller!= null)
			{
                _Controller.Logoff  ();
                _Controller.Dispose();
			}

            scanner.Scan();
            ABB.Robotics.Controllers.Discovery.NetworkScanner.AddRemoteController(_IPAddress);
            controllerInfos = scanner.Controllers;


			foreach (ControllerInfo item in controllerInfos)
			{
				if (item.IPAddress==_IPAddress)
				{
                    _Controller = Controller.Connect(item, ConnectionType.Standalone);
                    _Controller.Logon(UserInfo.DefaultUser);
                    return true;
				}
			}
            return false;

		}

        protected void Robot_Check(object sender, EventArgs e)
		{
			if (_Controller==null)
			{
				_RobState= Robot_Status.Disconnected;
				return;
			}

			if (_Controller.State != ControllerState.MotorsOn)
			{
				_RobState = Robot_Status.Motors_Off;
				return;
			}

			foreach (ABB.Robotics.Controllers.RapidDomain.Task RobTask in _Controller.Rapid.GetTasks())
			{
				if (RobTask.ExecutionStatus == ABB.Robotics.Controllers.RapidDomain.TaskExecutionStatus.Running &&
					RobTask.TaskType == ABB.Robotics.Controllers.RapidDomain.TaskType.Normal)
				{
					_RobState = Robot_Status.Running;
					return;
				}
			}

			if (_Controller.OperatingMode == ControllerOperatingMode.ManualReducedSpeed)
			{
				_RobState = Robot_Status.Manual_Mode;
				return;
			}

			if (_Controller.OperatingMode == ControllerOperatingMode.Auto && _Controller.State == ControllerState.MotorsOn
				)
			{
				_RobState = Robot_Status.Running;
			}

		}


    }
}
