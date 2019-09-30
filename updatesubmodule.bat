git submodule update --init --recursive
git submodule foreach --recursive git reset --hard
git submodule foreach --recursive git pull origin master