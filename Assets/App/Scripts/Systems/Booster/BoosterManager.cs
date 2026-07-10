using UnityEngine;

namespace MagicTile.Booster
{
    public class BoosterManager : IBoosterService
    {
        private BoosterModel _model;
        private BoosterPresenter _presenter;

        public BoosterManager()
        {
            _model = new BoosterModel();
            _presenter = new BoosterPresenter(_model);
        }

        ~BoosterManager()
        {
            if (ServiceLocator.ServiceLocator.IsRegistered<IBoosterService>())
                ServiceLocator.ServiceLocator.Unregister<IBoosterService>();
        }

        public void RunBooster(BoosterType type)
        {
            _presenter.RunBooster(type);
        }
    }
}
