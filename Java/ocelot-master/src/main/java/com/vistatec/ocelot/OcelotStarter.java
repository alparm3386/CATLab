package com.vistatec.ocelot;

import com.google.inject.Guice;
import com.google.inject.Injector;
import com.vistatec.ocelot.di.OcelotModule;

import javax.swing.*;
import java.awt.*;
import java.io.File;
import java.io.IOException;

public class OcelotStarter {
    public Ocelot ocelot;

    public void OcelotStarter() throws IOException, InstantiationException, IllegalAccessException {
    }

    public void start() throws IOException, InstantiationException, IllegalAccessException {
        Injector ocelotScope = Guice.createInjector(new OcelotModule());

        ocelot = new Ocelot(ocelotScope);
        DefaultKeyboardFocusManager.getCurrentKeyboardFocusManager().addKeyEventDispatcher(ocelot);

        try {
            if (ocelot.getUseNativeUI()) {
                UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
            } else {
                UIManager.setLookAndFeel(UIManager.getCrossPlatformLookAndFeelClassName());
            }
        } catch (Exception e) {
            System.err.println(e.getMessage());
        }
        SwingUtilities.invokeLater(ocelot);
    }

    public void openFile(String sPath) {
        ocelot.openFile(new File(sPath), true);
    }
}
