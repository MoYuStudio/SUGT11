using System;
using Yukar.Engine;

namespace SharpKmyBase
{
    public class Task
    {

        protected static void shutdown()
        {
            throw new NotImplementedException();
        }

        public virtual void update(float elapsed)
        {

        }

        public virtual void initialize()
        {

        }

        public virtual void finalize()
        {

        }

        //���j���[���Q�[�����I������
        protected static void removeTask(GameMain gameMain)
        {
            // Unity�ł̓^�C�g���ɖ߂�悤�ɂ���
            gameMain.ChangeScene(GameMain.Scenes.TITLE);
        }
    }
}