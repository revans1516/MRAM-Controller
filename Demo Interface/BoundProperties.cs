using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Demo_Interface
{
	public class BoundProperties : INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChangedl([CallerMemberName] string propertyName="")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		private RobotConnection.Robot_Status _PingStatus;
		public	RobotConnection.Robot_Status PingStatus
		{
			get { return _PingStatus; }
		}


		private RobotConnection.Robot_Status _TickStatus;
		public RobotConnection.Robot_Status TicklStatus
		{
			get { return _TickStatus; }
		}

		private RobotConnection.Robot_Status _RazzStatus;
		public RobotConnection.Robot_Status RazzStatus
		{
			get { return _RazzStatus; }
		}



	}
}
