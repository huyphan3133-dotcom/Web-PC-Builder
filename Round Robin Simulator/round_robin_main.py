import sys

from PySide6.QtCore import QFile
from PySide6.QtUiTools import QUiLoader
from PySide6.QtWidgets import QApplication

from round_robin_controller import RoundRobinController


def main() -> None:
    app = QApplication(sys.argv)        

    ui_file = QFile("RR.ui")

    if not ui_file.open(
        QFile.OpenModeFlag.ReadOnly
    ):
        raise RuntimeError(
            "không thể mở file RR.ui"
        );

    loader = QUiLoader()
    window = loader.load(ui_file)
    ui_file.close()

    if window is None:
       raise RuntimeError(
                   "không thể tải giao diện RR.ui"
               );

    controller = RoundRobinController(window)

    window.show()
    sys.exit(app.exec())

def new_func2():
    new_func()

def new_func1():
    raise RuntimeError(
            "Không thể tải giao diện RR.ui"
        )

def new_func():
    raise RuntimeError(
            "Không thể mở file RR.ui"
        )


def new_func3(main):
    main()

