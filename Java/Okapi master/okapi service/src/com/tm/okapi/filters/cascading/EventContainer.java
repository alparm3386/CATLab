package com.tm.okapi.filters.cascading;

import net.sf.okapi.common.Event;
import net.sf.okapi.common.IResource;
import net.sf.okapi.common.annotation.Annotations;
import net.sf.okapi.common.resource.Property;
import  net.sf.okapi.common.resource.Custom;

import java.util.Map;

public class EventContainer extends Custom {
    private Event event;
    private String id;

    public EventContainer(String id, Event event) {
        super();
        this.id = id;
        this.event = event;
    }

    Event getEvent() {
        return event;
    }

    @Override
    public String getId() {
        return id;
    }

    @Override
    public void setId(String id) {
        this.id = id;
    }

    @Override
    public Annotations getAnnotations() {
        return super.getAnnotations();
    }

    @Override
    public Map<String, Property> getProperties() {
        return super.getProperties();
    }
}
